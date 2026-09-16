using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Samples.Builder2;

public partial class GenerateBuilderAttribute
{
    [CompileTime]
    private class PropertyMapping
    {
        public PropertyMapping( IProperty sourceProperty, bool isRequired, bool isInherited )
        {
            this.SourceProperty = sourceProperty;
            this.IsRequired = isRequired;
            this.IsInherited = isInherited;
        }

        public IProperty SourceProperty { get; }

        public bool IsRequired { get; }

        public bool IsInherited { get; }

        public IProperty? BuilderProperty { get; set; }

        public int? SourceConstructorParameterIndex { get; set; }

        public int? BuilderConstructorParameterIndex { get; set; }
    }
}