using Metalama.Samples.Authorization.Demo.Data;
using Metalama.Samples.Authorization.RoleBased;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// The rules of the policy are seed data of the database, and the policy loads them from there.
/// </summary>
public class PolicyStorageTests : IAsyncLifetime
{
    private DemoOrganization _org = null!;

    public async ValueTask InitializeAsync() => this._org = await DemoOrganization.CreateAsync();

    public async ValueTask DisposeAsync() => await this._org.DisposeAsync();

    private DatabaseAuthorizationPolicy Policy => this._org.RootServices.GetRequiredService<DatabaseAuthorizationPolicy>();

    [Fact]
    public async Task Rules_AreStoredInTheDatabase()
    {
        var rules = await this._org.Db.PermissionRules.AsNoTracking().ToListAsync();

        Assert.Equal( DemoPolicy.CreateSeedRules().Count, rules.Count );
        Assert.Contains( rules, r => r is { ObjectType: "Invoice", Permission: DemoPermissions.Approve, Role: "Owner", Effect: PermissionEffect.Deny } );
        Assert.Contains( rules, r => r is { ObjectType: PermissionRuleRecord.AnyType, Permission: PermissionNames.Read, Role: "Auditor" } );
    }

    [Fact]
    public async Task ChangedRule_TakesEffectAfterReload()
    {
        using ( this._org.SignIn( this._org.Sam ) )
        {
            _ = this._org.SalesBudget.Status;
        }

        // An administrator removes the rule that lets assistants see the status of the budget.
        var rule = await this._org.Db.PermissionRules.SingleAsync(
            r => r.ObjectType == "Budget" && r.Permission == PermissionNames.Read && r.Role == nameof(DemoRoles.DepartmentAssistant) );

        this._org.Db.PermissionRules.Remove( rule );
        await this._org.SaveChangesAsync();
        await this.Policy.ReloadAsync( this._org.Db );

        using ( this._org.SignIn( this._org.Sam ) )
        {
            Assert.Throws<AccessDeniedException>( () => this._org.SalesBudget.Status );
        }
    }

    [Theory]
    [InlineData( "Auditer", PermissionNames.Write )]
    [InlineData( "Auditor", "Wirte" )]
    public async Task DenyRuleWithAnUnknownName_MakesTheReloadFail( string role, string permission )
    {
        // Loaded without an error, a mistyped Deny rule would match no role or no check, and the auditors could write.
        this._org.Db.PermissionRules.Add( new PermissionRuleRecord( PermissionRuleRecord.AnyType, permission, role, PermissionEffect.Deny ) );
        await this._org.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>( () => this.Policy.ReloadAsync( this._org.Db ) );
    }

    [Fact]
    public async Task RuleOnAnUnknownClass_MakesTheReloadFail()
    {
        this._org.Db.PermissionRules.Add( new PermissionRuleRecord( "Contract", PermissionNames.Read, "Auditor", PermissionEffect.Allow ) );
        await this._org.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>( () => this.Policy.ReloadAsync( this._org.Db ) );

        // The previous rules stay in force.
        using ( this._org.SignIn( this._org.Sam ) )
        {
            _ = this._org.SalesBudget.Status;
        }
    }

    [Fact]
    public void PolicyWithoutRules_DeniesEverything()
    {
        var policy = new DatabaseAuthorizationPolicy();

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.False( policy.IsGranted( this._org.Mia, PermissionNames.Read, this._org.SalesInvoice ) );
        }
    }
}
