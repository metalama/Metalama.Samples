using Metalama.Samples.Authorization.Demo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Metalama.Samples.Authorization.Demo.Services;

/// <summary>
/// Runs a request on behalf of a user: it creates a dependency injection scope with its own database context,
/// loads the user, and sets <see cref="SecurityContext.Current"/> while the request runs. In a web application,
/// a middleware plays this role after authentication.
/// </summary>
public sealed class RequestRunner( IServiceScopeFactory scopeFactory, IAuthorizationPolicy policy, IAccessAuditor auditor )
{
    public async Task<TResult> RunAsync<TResult>(
        string userName,
        Func<IServiceProvider, Task<TResult>> request,
        CancellationToken cancellationToken = default )
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
        var user = await db.Users.SingleAsync( u => u.Name == userName, cancellationToken );

        // SecurityContext.Current is stored in an AsyncLocal, so it must be set here, in the method that awaits
        // the request. A value set inside an awaited method is not visible to its caller.
        using ( SecurityContext.Use( new UserSecurityContext( user, policy, auditor ) ) )
        {
            return await request( scope.ServiceProvider );
        }
    }

    public Task RunAsync( string userName, Func<IServiceProvider, Task> request, CancellationToken cancellationToken = default )
        => this.RunAsync(
            userName,
            async services =>
            {
                await request( services );

                return true;
            },
            cancellationToken );
}
