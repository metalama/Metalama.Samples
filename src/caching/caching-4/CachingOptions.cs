using Metalama.Framework.Code;
using Metalama.Framework.Options;

public record CachingOptions :
    IHierarchicalOptions<IMethod>,
    IHierarchicalOptions<INamedType>,
    IHierarchicalOptions<INamespace>,
    IHierarchicalOptions<ICompilation>
{
    private readonly IncrementalKeyedCollection<string, CacheBuilderRegistration>
        _cacheBuilderRegistrations;

    public static CachingOptions Default { get; } = new();

    public CachingOptions() : this(
        IncrementalKeyedCollection<string, CacheBuilderRegistration>
            .Empty ) { }

    private CachingOptions( IncrementalKeyedCollection<string, CacheBuilderRegistration> cacheBuilderRegistrations )
    {
        this._cacheBuilderRegistrations = cacheBuilderRegistrations;
    }

    internal IEnumerable<CacheBuilderRegistration> Registrations => this._cacheBuilderRegistrations;

    public CachingOptions UseToString( Type type )
        => new(
            this._cacheBuilderRegistrations.AddOrApplyChanges(
                new CacheBuilderRegistration( TypeFactory.GetType( type ), null ) ) );

    public CachingOptions UseCacheKeyBuilder( Type type, Type builderType )
        => new(
            this._cacheBuilderRegistrations.AddOrApplyChanges(
                new CacheBuilderRegistration(
                    TypeFactory.GetType( type ),
                    TypeFactory.GetType( builderType ) ) ) );

    public object ApplyChanges( object changes, in ApplyChangesContext context )
        => new CachingOptions(
            this._cacheBuilderRegistrations.AddOrApplyChanges(
                ((CachingOptions) changes)._cacheBuilderRegistrations ) );
}