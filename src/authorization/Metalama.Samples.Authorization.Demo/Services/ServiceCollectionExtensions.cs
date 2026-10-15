using Metalama.Samples.Authorization.Demo.Data;
using Metalama.Samples.Authorization.Demo.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Metalama.Samples.Authorization.Demo.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the invoicing application: the database, the security infrastructure and the application services.
    /// </summary>
    public static IServiceCollection AddInvoicingApplication( this IServiceCollection services )
    {
        // Each request gets its own database context and connection. The database is a named in-memory SQLite
        // database, shared by all the connections and kept alive by InMemoryDatabase.
        services.AddSingleton<InMemoryDatabase>();

        // Split queries load each collection navigation, such as role assignments, with its own SQL query.
        // Entities that Entity Framework loads are created in a system scope.
        services.AddDbContext<InvoicingDbContext>(
            ( provider, options ) => options
                .UseSqlite(
                    provider.GetRequiredService<InMemoryDatabase>().ConnectionString,
                    sqlite => sqlite.UseQuerySplittingBehavior( QuerySplittingBehavior.SplitQuery ) )
                .AddInterceptors( SystemScopeMaterializationInterceptor.Instance ) );

        // Security infrastructure. The policy loads its rules from the database, see DemoData.
        services.AddSingleton<DatabaseAuthorizationPolicy>();
        services.AddSingleton<IAuthorizationPolicy>( provider => provider.GetRequiredService<DatabaseAuthorizationPolicy>() );
        services.AddSingleton<IAccessAuditor, LoggingAccessAuditor>();
        services.AddSingleton<RequestRunner>();

        // Infrastructure services used by the domain.
        services.AddSingleton<OutboxInvoiceSender>();
        services.AddSingleton<IInvoiceSender>( provider => provider.GetRequiredService<OutboxInvoiceSender>() );

        // Application services, one instance per request.
        services.AddScoped<InvoiceService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<BudgetService>();
        services.AddScoped<StaffService>();

        return services;
    }
}
