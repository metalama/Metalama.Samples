using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Samples.Builder3;

[CompileTime]
internal abstract partial class PropertyMapping : ITemplateProvider
{
    protected PropertyMapping( IProperty sourceProperty, bool isRequired, bool isInherited )
    {
        this.SourceProperty = sourceProperty;
        this.IsRequired = isRequired;
        this.IsInherited = isInherited;
    }

    public IProperty SourceProperty { get; }

    public bool IsRequired { get; }

    public bool IsInherited { get; }

    public int? SourceConstructorParameterIndex { get; set; }

    public int? BuilderConstructorParameterIndex { get; set; }

    /// <summary>
    /// Gets an expression that contains the value of the Builder property. The type of the
    /// expression must be the type of the property in the <i>source</i> type, not in the builder
    /// type.
    /// </summary>
    public abstract IExpression GetBuilderPropertyValue();

    /// <summary>
    /// Adds the properties, fields and methods required to implement this property.
    /// </summary>
    public abstract void ImplementBuilderArtifacts( IAdviser<INamedType> builderType );

    /// <summary>
    /// Imports, from the base type, the properties, field and methods required for
    /// the current property. 
    /// </summary>
    public abstract bool TryImportBuilderArtifactsFromBaseType(
        INamedType baseType,
        ScopedDiagnosticSink diagnosticSink );

    /// <summary>
    /// A template for the code that sets the relevant data in the Builder type for the current property. 
    /// </summary>
    [Template]
    public virtual void SetBuilderPropertyValue( IExpression expression, IExpression builderInstance )
    {
        // Abstract templates are not supported, so we must create a virtual method and override it.
        throw new NotSupportedException();
    }
}