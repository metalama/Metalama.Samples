using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Metalama.Samples.Authorization;

/// <summary>
/// An <see cref="IAuthorizationPolicy"/> that caches the decisions of another policy in an <see cref="IMemoryCache"/>,
/// for all the execution contexts of the process. Each execution context has its own cache of decisions (see
/// <see cref="SecurityContext"/>), so this policy is called only once per execution context for an object and a
/// permission, and each operation runs with a consistent set of decisions.
/// </summary>
/// <remarks>
/// <para>The cache is invalidated manually. Call <see cref="Invalidate"/> after a change of the data that the policy
/// reads for an object, for instance its owner or its role assignments, and <see cref="InvalidateAll"/> after a change
/// of the rules. Invalidating an object also invalidates the decisions on the objects that depend on it, as returned
/// by the function given to the constructor, for instance the objects that inherit their roles from it.</para>
/// <para>The keys identify the objects and the subjects, not their instances, because an application such as a web
/// application loads new instances in each request. An object or a subject without a key, for instance an object
/// that has not been saved yet, is not cached.</para>
/// <para>A decision is cached only when the data that it was made from is current. Each invalidation increments
/// <see cref="Generation"/>, and the application calls <see cref="RecordLoad"/> when it loads an object. A decision on
/// an object, or on an object that depends on another one, that was loaded before the last invalidation may have been
/// made from data that has changed since then, so it is not cached. An object whose load was not recorded is not
/// cached either.</para>
/// <para>An execution context that started before an invalidation keeps its own decisions until it ends.</para>
/// </remarks>
public sealed class CachingAuthorizationPolicy : IAuthorizationPolicy, IDisposable
{
    private readonly IAuthorizationPolicy _inner;
    private readonly IMemoryCache _cache;
    private readonly Func<object, object?> _getKey;
    private readonly Func<ISecurable, IEnumerable<ISecurable>> _getDependencies;
    private readonly TimeSpan _expiration;
    private readonly ConditionalWeakTable<object, StrongBox<long>> _loadGenerations = new();
    private readonly Lock _tokenLock = new();
    private CancellationTokenSource _allToken = new();
    private long _generation;

    /// <summary>
    /// Initializes a new <see cref="CachingAuthorizationPolicy"/>.
    /// </summary>
    /// <param name="inner">The policy whose decisions are cached.</param>
    /// <param name="cache">The memory cache. It can be shared with other components: the keys of this policy are private.</param>
    /// <param name="getKey">Returns the key that identifies a subject or a securable object, or <c>null</c> when the
    /// decisions on it must not be cached.</param>
    /// <param name="getDependencies">Returns the objects whose data the policy reads to decide on an object, in
    /// addition to the object itself, for instance its parents. It runs in a system scope.</param>
    /// <param name="expiration">The time after which a decision expires, even without invalidation.</param>
    public CachingAuthorizationPolicy(
        IAuthorizationPolicy inner,
        IMemoryCache cache,
        Func<object, object?> getKey,
        Func<ISecurable, IEnumerable<ISecurable>>? getDependencies = null,
        TimeSpan? expiration = null )
    {
        this._inner = inner;
        this._cache = cache;
        this._getKey = getKey;
        this._getDependencies = getDependencies ?? (_ => []);
        this._expiration = expiration ?? TimeSpan.FromMinutes( 5 );
    }

    /// <summary>
    /// Gets the number of invalidations since the creation of the policy.
    /// </summary>
    public long Generation => Interlocked.Read( ref this._generation );

    /// <summary>
    /// Records that an object has just been loaded, with the current data of the store. Call it before the data of the
    /// object is read, for instance when the object is created by the data access layer.
    /// </summary>
    public void RecordLoad( object value ) => this._loadGenerations.AddOrUpdate( value, new StrongBox<long>( this.Generation ) );

    public bool IsGranted( ISubject subject, Permission permission, ISecurable securable )
    {
        var subjectKey = this._getKey( subject );
        var securableKey = this._getKey( securable );

        if ( subjectKey == null || securableKey == null )
        {
            return this._inner.IsGranted( subject, permission, securable );
        }

        var key = new CacheKey( subjectKey, securableKey, permission.Name );

        if ( this._cache.TryGetValue( key, out bool isGranted ) )
        {
            return isGranted;
        }

        var dependencies = this._getDependencies( securable ).ToList();

        // The tokens are taken before the decision: when an invalidation happens while the inner policy runs, the
        // tokens are already canceled when the entry is added, and the entry expires immediately.
        var tokens = this.GetTokens( securableKey, dependencies );

        isGranted = this._inner.IsGranted( subject, permission, securable );

        if ( tokens != null && this.IsCurrent( securable, dependencies ) )
        {
            // The size is required when the cache has a size limit.
            var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = this._expiration, Size = 1 };

            foreach ( var token in tokens )
            {
                options.ExpirationTokens.Add( token );
            }

            this._cache.Set( key, isGranted, options );
        }

        return isGranted;
    }

    /// <summary>
    /// Removes the cached decisions on an object, and on the objects that depend on it.
    /// </summary>
    public void Invalidate( ISecurable securable )
    {
        var key = this._getKey( securable );

        if ( key == null )
        {
            return;
        }

        lock ( this._tokenLock )
        {
            Interlocked.Increment( ref this._generation );

            if ( this._cache.TryGetValue( new TokenKey( key ), out CancellationTokenSource? source ) )
            {
                this._cache.Remove( new TokenKey( key ) );

                // The token is canceled here, and not in the eviction callback, which runs asynchronously.
                source!.Cancel();
                source.Dispose();
            }
        }
    }

    /// <summary>
    /// Removes all the cached decisions, for instance after a change of the rules of the policy.
    /// </summary>
    public void InvalidateAll()
    {
        lock ( this._tokenLock )
        {
            Interlocked.Increment( ref this._generation );

            var previous = this._allToken;
            this._allToken = new CancellationTokenSource();
            previous.Cancel();
            previous.Dispose();
        }
    }

    public void Dispose()
    {
        lock ( this._tokenLock )
        {
            this._allToken.Dispose();
        }
    }

    // A decision is made from the data of the object and of its dependencies, so all of them must have been loaded
    // after the last invalidation.
    private bool IsCurrent( ISecurable securable, List<ISecurable> dependencies )
    {
        var generation = this.Generation;

        return this.IsLoadedIn( securable, generation ) && dependencies.TrueForAll( dependency => this.IsLoadedIn( dependency, generation ) );
    }

    private bool IsLoadedIn( object value, long generation )
        => this._loadGenerations.TryGetValue( value, out var loadGeneration ) && loadGeneration.Value >= generation;

    // Returns null when the decision cannot be cached because a dependency cannot be invalidated.
    private List<IChangeToken>? GetTokens( object securableKey, List<ISecurable> dependencies )
    {
        var dependencyKeys = new List<object>();

        foreach ( var dependency in dependencies )
        {
            var dependencyKey = this._getKey( dependency );

            if ( dependencyKey == null )
            {
                return null;
            }

            dependencyKeys.Add( dependencyKey );
        }

        // The tokens are read under the lock, so that no token source is disposed while its token is read.
        lock ( this._tokenLock )
        {
            var tokens = new List<IChangeToken> { new CancellationChangeToken( this._allToken.Token ), this.GetObjectToken( securableKey ) };

            foreach ( var dependencyKey in dependencyKeys )
            {
                tokens.Add( this.GetObjectToken( dependencyKey ) );
            }

            return tokens;
        }
    }

    // The token of an object is stored in the cache too, so that the tokens of the objects that are no longer used are
    // removed. When the token is evicted, it is canceled, so that no decision survives the token that invalidates it.
    // The caller holds the lock, which makes sure that only one token exists for an object: a decision bound to a
    // second token would not be invalidated.
    private CancellationChangeToken GetObjectToken( object key )
    {
        if ( !this._cache.TryGetValue( new TokenKey( key ), out CancellationTokenSource? source ) )
        {
            source = new CancellationTokenSource();

            var options = new MemoryCacheEntryOptions { SlidingExpiration = 2 * this._expiration, Size = 1 };

            // Invalidate removes the entry after it has canceled and disposed the token source. Any other eviction
            // cancels it here, under the lock, so that it is not disposed while another thread reads its token.
            options.RegisterPostEvictionCallback(
                ( _, value, reason, _ ) =>
                {
                    if ( reason != EvictionReason.Removed )
                    {
                        lock ( this._tokenLock )
                        {
                            var evicted = (CancellationTokenSource) value!;
                            evicted.Cancel();
                            evicted.Dispose();
                        }
                    }
                } );

            this._cache.Set( new TokenKey( key ), source, options );
        }

        return new CancellationChangeToken( source!.Token );
    }

    // Private types, so that the keys cannot collide with the keys of other components that share the cache.
    private sealed record CacheKey( object Subject, object Securable, string Permission );

    private sealed record TokenKey( object Securable );
}
