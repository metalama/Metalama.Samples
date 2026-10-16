using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Metalama.Samples.Authorization.Demo.Data;

/// <summary>
/// Invalidates the decisions of <see cref="CachingAuthorizationPolicy"/> after a save that changes the data that the
/// policy reads.
/// </summary>
/// <remarks>
/// <para>A changed or deleted entity is invalidated: its owner or its parent may have changed. A change of the role
/// assignments invalidates all the decisions, because the roles of an object apply to all its descendants. Role
/// assignments change rarely, so this costs little.</para>
/// <para>The invalidation runs after the save, when the new data is visible to the other requests.</para>
/// </remarks>
public sealed class PermissionCacheInvalidationInterceptor( CachingAuthorizationPolicy cache ) : SaveChangesInterceptor
{
    private readonly ConditionalWeakTable<DbContext, PendingInvalidation> _pending = new();

    public override InterceptionResult<int> SavingChanges( DbContextEventData eventData, InterceptionResult<int> result )
    {
        this.Collect( eventData.Context );

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default )
    {
        this.Collect( eventData.Context );

        return ValueTask.FromResult( result );
    }

    public override int SavedChanges( SaveChangesCompletedEventData eventData, int result )
    {
        this.Invalidate( eventData.Context );

        return result;
    }

    public override ValueTask<int> SavedChangesAsync( SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default )
    {
        this.Invalidate( eventData.Context );

        return ValueTask.FromResult( result );
    }

    public override void SaveChangesFailed( DbContextErrorEventData eventData )
    {
        if ( eventData.Context != null )
        {
            this._pending.Remove( eventData.Context );
        }
    }

    public override Task SaveChangesFailedAsync( DbContextErrorEventData eventData, CancellationToken cancellationToken = default )
    {
        this.SaveChangesFailed( eventData );

        return Task.CompletedTask;
    }

    private void Collect( DbContext? context )
    {
        if ( context == null )
        {
            return;
        }

        var tracker = context.ChangeTracker;
        tracker.DetectChanges();

        var pending = new PendingInvalidation(
            tracker.Entries<Entity>().Where( e => e.State is EntityState.Modified or EntityState.Deleted ).Select( e => e.Entity ).ToList(),
            tracker.Entries<RoleAssignmentRecord>().Any( e => e.State is not EntityState.Unchanged ) );

        this._pending.AddOrUpdate( context, pending );
    }

    private void Invalidate( DbContext? context )
    {
        if ( context == null || !this._pending.TryGetValue( context, out var pending ) )
        {
            return;
        }

        this._pending.Remove( context );

        if ( pending.RolesChanged )
        {
            cache.InvalidateAll();

            return;
        }

        foreach ( var entity in pending.Entities )
        {
            cache.Invalidate( entity );
        }
    }

    private sealed record PendingInvalidation( List<Entity> Entities, bool RolesChanged );
}
