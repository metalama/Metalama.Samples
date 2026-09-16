using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Samples.Builder3;

internal class StandardPropertyMapping : PropertyMapping
{
    private IProperty? _builderProperty;

    public StandardPropertyMapping( IProperty sourceProperty, bool isRequired, bool isInherited )
        : base( sourceProperty, isRequired, isInherited ) { }

    public override IExpression GetBuilderPropertyValue() => this._builderProperty!;

    public override void ImplementBuilderArtifacts( IAdviser<INamedType> builderType )
    {
        this._builderProperty = builderType.IntroduceAutomaticProperty(
                this.SourceProperty.Name,
                this.SourceProperty.Type,
                IntroductionScope.Instance,
                buildProperty: p =>
                {
                    p.Accessibility = Accessibility.Public;
                    p.InitializerExpression = this.SourceProperty.InitializerExpression;
                } )
            .Declaration;
    }

    public override bool TryImportBuilderArtifactsFromBaseType(
        INamedType baseType,
        ScopedDiagnosticSink diagnosticSink )

    {
        return this.TryFindBuilderPropertyInBaseType(
            baseType,
            diagnosticSink,
            out this._builderProperty );
    }

    public override void SetBuilderPropertyValue(
        IExpression expression,
        IExpression builderInstance )
    {
        this._builderProperty!.WithObject( builderInstance ).Value = expression.Value;
    }
}