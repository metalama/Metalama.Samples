using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Samples.Builder1;

public partial class GenerateBuilderAttribute
{
    // [<snippet Tags>]
    [CompileTime]
    private record Tags(
        INamedType SourceType,
        IReadOnlyList<PropertyMapping> Properties,
        IConstructor SourceConstructor,
        IConstructor BuilderCopyConstructor );

    [CompileTime]
    private class PropertyMapping
    {
        public PropertyMapping( IProperty sourceProperty, bool isRequired )
        {
            this.SourceProperty = sourceProperty;
            this.IsRequired = isRequired;
        }

        public IProperty SourceProperty { get; }

        public bool IsRequired { get; }

        public IProperty? BuilderProperty { get; set; }

        public int? SourceConstructorParameterIndex { get; set; }

        public int? BuilderConstructorParameterIndex { get; set; }
    }

    // [<endsnippet Tags>]
}