using Metalama.Framework.Code;
using Metalama.Framework.Options;

internal class CacheBuilderRegistration : IIncrementalKeyedCollectionItem<string>
{
    public CacheBuilderRegistration( IType keyType, IType? builderType )
    {
        // We are using ToDisplayString for the key and not SerializableTypeId because
        // SerializableTypeId is too strict -- it includes too much nullability information.
        this.KeyType = keyType.ToDisplayString( CodeDisplayFormat.FullyQualified );
        this.BuilderType = builderType?.ToSerializableId();
    }

    public object ApplyChanges( object changes, in ApplyChangesContext context ) => changes;

    public string KeyType { get; }

    public SerializableTypeId? BuilderType { get; }

    public bool UseToString => this.BuilderType == null;

    string IIncrementalKeyedCollectionItem<string>.Key => this.KeyType;
}