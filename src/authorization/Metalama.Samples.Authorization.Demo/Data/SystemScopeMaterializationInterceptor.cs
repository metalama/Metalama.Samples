using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Metalama.Samples.Authorization.Demo.Data;

/// <summary>
/// Creates the entities that Entity Framework loads in a system scope. Loading an object is an infrastructure
/// operation, not a user operation: its constructor can set secured properties, for instance by chaining to the
/// constructor that the application uses, without a permission and without a security context.
/// </summary>
/// <remarks>
/// <para>Each entity needs a parameterless constructor, which can be private.</para>
/// <para>The scope covers only the creation of the object. After it, Entity Framework sets the values through the
/// backing fields (see <see cref="InvoicingDbContext"/>), which runs no check either.</para>
/// </remarks>
public sealed class SystemScopeMaterializationInterceptor : IMaterializationInterceptor
{
    public static SystemScopeMaterializationInterceptor Instance { get; } = new();

    public InterceptionResult<object> CreatingInstance( MaterializationInterceptionData materializationData, InterceptionResult<object> result )
    {
        var type = materializationData.EntityType.ClrType;

        if ( !typeof(Entity).IsAssignableFrom( type ) )
        {
            return result;
        }

        // The using statement closes the scope even if the constructor throws.
        using ( SecurityContext.RunAsSystem() )
        {
            try
            {
                return InterceptionResult<object>.SuppressWithResult( Activator.CreateInstance( type, nonPublic: true )! );
            }
            catch ( MissingMethodException e )
            {
                throw new InvalidOperationException( $"The entity {type.Name} needs a parameterless constructor, which can be private.", e );
            }
        }
    }
}
