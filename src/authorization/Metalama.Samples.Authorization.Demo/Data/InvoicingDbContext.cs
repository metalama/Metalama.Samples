using Metalama.Samples.Authorization.Demo.Finance;
using Metalama.Samples.Authorization.Demo.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Metalama.Samples.Authorization.Demo.Data;

/// <summary>
/// The Entity Framework model of the invoicing application (code first).
/// </summary>
public sealed class InvoicingDbContext( DbContextOptions<InvoicingDbContext> options, CachingAuthorizationPolicy cachingPolicy )
    : DbContext( options )
{
    /// <summary>
    /// Gets the shared cache of decisions, which the interceptors of the context maintain. The interceptors are
    /// single instances, because Entity Framework creates an internal service provider for each set of interceptor
    /// instances, so they find the cache through the context.
    /// </summary>
    internal CachingAuthorizationPolicy CachingPolicy { get; } = cachingPolicy;

    public DbSet<User> Users => this.Set<User>();

    public DbSet<Company> Companies => this.Set<Company>();

    public DbSet<Department> Departments => this.Set<Department>();

    public DbSet<Customer> Customers => this.Set<Customer>();

    public DbSet<Invoice> Invoices => this.Set<Invoice>();

    public DbSet<Budget> Budgets => this.Set<Budget>();

    public DbSet<PermissionRuleRecord> PermissionRules => this.Set<PermissionRuleRecord>();

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        // Entity Framework must never go through the secured accessors. Loading an object, detecting changes and
        // saving are infrastructure operations, not user operations: they often run without a security context,
        // and they read and write every property. Field access makes Entity Framework read and write the backing
        // fields directly, including the fields that Metalama generates for the overridden auto-properties.
        modelBuilder.UsePropertyAccessMode( PropertyAccessMode.Field );

        // By convention, Entity Framework maps only the properties that have a setter. The get-only properties that
        // expose the state of an object, such as Invoice.Status, must be mapped explicitly, or they are not saved.

        modelBuilder.Entity<User>( user => user.HasIndex( u => u.Name ).IsUnique() );

        // The rules of the policy are stored by name, and the effect as text, so that an administrator can read them.
        modelBuilder.Entity<PermissionRuleRecord>( rule => rule.Property( r => r.Effect ).HasConversion<string>() );

        ConfigureSecurable<Company>( modelBuilder );

        ConfigureSecurable<Department>( modelBuilder )
            .Entity<Department>(
                department =>
                {
                    department.Property( d => d.Code );
                    department.HasIndex( d => d.Code ).IsUnique();
                    department.HasOne( d => d.Company ).WithMany();
                    department.Property<int>( nameof(Department.LastInvoiceNumber) );
                } );

        ConfigureSecurable<Customer>( modelBuilder )
            .Entity<Customer>( customer => customer.HasOne( c => c.Department ).WithMany() );

        ConfigureSecurable<Invoice>( modelBuilder )
            .Entity<Invoice>(
                invoice =>
                {
                    invoice.Property( i => i.Number );
                    invoice.Property( i => i.Status );
                    invoice.Property( i => i.AmountPaid );
                    invoice.HasIndex( i => i.Number ).IsUnique();
                    invoice.HasOne( i => i.Department ).WithMany();
                    invoice.HasOne( i => i.Customer ).WithMany();
                    invoice.HasOne<User>( "CreatorUser" ).WithMany();
                    invoice.Ignore( i => i.Creator );
                    invoice.OwnsMany( i => i.Lines, line => UseSingleColumnKey( line ) );
                    // Entity Framework assigns a new collection when it loads a primitive collection. The converter
                    // makes this collection a securable list, as the constructor does.
                    invoice.Property( i => i.Notes )
                        .HasConversion(
                            notes => JsonSerializer.Serialize( notes, (JsonSerializerOptions?) null ),
                            json => DeserializeNotes( json ),
                            new ValueComparer<IList<string>>(
                                ( left, right ) => left!.SequenceEqual( right! ),
                                notes => notes.Aggregate( 0, ( hash, note ) => HashCode.Combine( hash, note ) ),
                                notes => CopyNotes( notes ) ) );
                } );

        ConfigureSecurable<Budget>( modelBuilder )
            .Entity<Budget>(
                budget =>
                {
                    budget.HasOne( b => b.Department ).WithMany();
                    budget.Property( b => b.FiscalYear );
                    budget.Property( b => b.Amount );
                    budget.Property( b => b.Spent );
                } );

        // The security model walks the parent chain and compares owners and role assignments. Loading these
        // navigations automatically means that a role inherited from an unloaded parent is never missed.
        modelBuilder.Entity<Department>().Navigation( d => d.Company ).AutoInclude();
        modelBuilder.Entity<Customer>().Navigation( c => c.Department ).AutoInclude();
        modelBuilder.Entity<Invoice>().Navigation( i => i.Department ).AutoInclude();
        modelBuilder.Entity<Invoice>().Navigation( i => i.Customer ).AutoInclude();
        modelBuilder.Entity<Invoice>().Navigation( "CreatorUser" ).AutoInclude();
        modelBuilder.Entity<Budget>().Navigation( b => b.Department ).AutoInclude();
    }

    // The lists are created in methods, and not in the expression trees of the converter: Metalama rewrites the
    // construction of a securable object to call its Initialize method, which an expression tree cannot contain.
    private static SecurableList<string> DeserializeNotes( string json )
        => new( JsonSerializer.Deserialize<List<string>>( json, (JsonSerializerOptions?) null ) ?? [] );

    private static SecurableList<string> CopyNotes( IList<string> notes ) => new( notes );

    // SQLite cannot generate the composite key that Entity Framework gives to owned collections by default.
    private static void UseSingleColumnKey<TOwner, TOwned>( OwnedNavigationBuilder<TOwner, TOwned> owned )
        where TOwner : class
        where TOwned : class
    {
        owned.Property<int>( "Id" );
        owned.HasKey( "Id" );
    }

    // Maps an entity: the storage of its owner and role assignments, by EfRoleBasedSecurable.
    private static ModelBuilder ConfigureSecurable<T>( ModelBuilder modelBuilder )
        where T : Entity
        => modelBuilder.Entity<T>(
            entity =>
            {
                entity.ConfigureRoleBasedStorage();
                entity.Ignore( e => e.SecurityLabel );
            } );
}
