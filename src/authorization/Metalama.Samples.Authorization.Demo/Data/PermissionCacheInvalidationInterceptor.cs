using System.Data.Common;
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
/// <para>The invalidation runs when the new data is visible to the other requests: after the save, or, when the save
/// runs in an explicit transaction, after the commit of the transaction. A rolled-back transaction invalidates
/// nothing.</para>
/// </remarks>
public sealed class PermissionCacheInvalidationInterceptor : SaveChangesInterceptor, IDbTransactionInterceptor
{
    public static PermissionCacheInvalidationInterceptor Instance { get; } = new();

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
        this.OnSaved( eventData.Context );

        return result;
    }

    public override ValueTask<int> SavedChangesAsync( SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default )
    {
        this.OnSaved( eventData.Context );

        return ValueTask.FromResult( result );
    }

    // A failed save outside of a transaction changed nothing. In a transaction, the changes of the previous saves are
    // still pending, and the end of the transaction decides.
    public override void SaveChangesFailed( DbContextErrorEventData eventData )
    {
        if ( eventData.Context is { Database.CurrentTransaction: null } context )
        {
            this._pending.Remove( context );
        }
    }

    public override Task SaveChangesFailedAsync( DbContextErrorEventData eventData, CancellationToken cancellationToken = default )
    {
        this.SaveChangesFailed( eventData );

        return Task.CompletedTask;
    }

    void IDbTransactionInterceptor.TransactionCommitted( DbTransaction transaction, TransactionEndEventData eventData ) => this.Invalidate( eventData.Context );

    Task IDbTransactionInterceptor.TransactionCommittedAsync( DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken )
    {
        this.Invalidate( eventData.Context );

        return Task.CompletedTask;
    }

    void IDbTransactionInterceptor.TransactionRolledBack( DbTransaction transaction, TransactionEndEventData eventData ) => this.Discard( eventData.Context );

    Task IDbTransactionInterceptor.TransactionRolledBackAsync( DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken )
    {
        this.Discard( eventData.Context );

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

        var entities = tracker.Entries<Entity>().Where( e => e.State is EntityState.Modified or EntityState.Deleted ).Select( e => e.Entity );
        var rolesChanged = tracker.Entries<RoleAssignmentRecord>().Any( e => e.State is not EntityState.Unchanged );

        // The saves of a transaction add up until the transaction ends.
        var pending = this._pending.GetValue( context, _ => new PendingInvalidation() );

        lock ( pending )
        {
            pending.Entities.AddRange( entities );
            pending.RolesChanged |= rolesChanged;
        }
    }

    private void OnSaved( DbContext? context )
    {
        if ( context is { Database.CurrentTransaction: null } )
        {
            this.Invalidate( context );
        }
    }

    private void Discard( DbContext? context )
    {
        if ( context != null )
        {
            this._pending.Remove( context );
        }
    }

    private void Invalidate( DbContext? context )
    {
        if ( context == null || !this._pending.TryGetValue( context, out var pending ) )
        {
            return;
        }

        this._pending.Remove( context );

        var cache = ((InvoicingDbContext) context).CachingPolicy;

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

    private sealed class PendingInvalidation
    {
        public List<Entity> Entities { get; } = [];

        public bool RolesChanged { get; set; }
    }
}
