using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Diagnostics;

namespace Metalama.Samples.Builder3;

internal class ImmutableCollectionPropertyMapping : PropertyMapping
{
    private IField? _collectionBuilderField;
    private IField? _initialValueField;
    private IProperty? _collectionBuilderProperty;
    private IMethod? _getImmutableValueMethod;
    private readonly IType _collectionBuilderType;

    public ImmutableCollectionPropertyMapping(
        IProperty sourceProperty,
        bool isRequired,
        bool isInherited ) : base( sourceProperty, isRequired, isInherited )
    {
        this._collectionBuilderType =
            ((INamedType) sourceProperty.Type).Types.OfName( "Builder" ).Single();
    }

    private IType ImmutableCollectionType => this.SourceProperty.Type;

    public override void ImplementBuilderArtifacts( IAdviser<INamedType> builderType )
    {
        builderType = builderType.WithTemplateProvider( this );

        this._collectionBuilderField = builderType
            .IntroduceField(
                NameHelper.ToFieldName( this.SourceProperty.Name + "Builder" ),
                this._collectionBuilderType.ToNullable(),
                buildField: f => f.Accessibility = Accessibility.Private )
            .Declaration;

        this._initialValueField = builderType
            .IntroduceField(
                NameHelper.ToFieldName( this.SourceProperty.Name ),
                this.ImmutableCollectionType,
                buildField: f =>
                {
                    f.Accessibility = Accessibility.Private;

                    if ( !this.IsRequired )
                    {
                        // Unless the field is required, we must initialize it to a value representing
                        // a valid but empty collection, except if we are given a different
                        // initializer expression.
                        if ( this.SourceProperty.InitializerExpression != null )
                        {
                            f.InitializerExpression = this.SourceProperty.InitializerExpression;
                        }
                        else
                        {
                            var initializerExpressionBuilder = new ExpressionBuilder();
                            initializerExpressionBuilder.AppendTypeName( this.ImmutableCollectionType );
                            initializerExpressionBuilder.AppendVerbatim( ".Empty" );
                            f.InitializerExpression = initializerExpressionBuilder.ToExpression();
                        }
                    }
                } )
            .Declaration;

        this._collectionBuilderProperty = builderType
            .IntroduceProperty(
                nameof(this.BuilderPropertyTemplate),
                buildProperty: p =>
                {
                    p.Name = this.SourceProperty.Name;
                    p.Accessibility = Accessibility.Public;
                    p.GetMethod!.Accessibility = Accessibility.Public;
                    p.Type = this._collectionBuilderType;
                } )
            .Declaration;

        this._getImmutableValueMethod = builderType.IntroduceMethod(
                nameof(this.BuildPropertyMethodTemplate),
                buildMethod: m =>
                {
                    m.Name = "GetImmutable" + this.SourceProperty.Name;
                    m.Accessibility = Accessibility.Protected;
                    m.ReturnType = this.ImmutableCollectionType;
                } )
            .Declaration;
    }

    [Template]
    public dynamic BuilderPropertyTemplate
        => this._collectionBuilderField!.Value ??= this._initialValueField!.Value!.ToBuilder();

    [Template]
    public dynamic BuildPropertyMethodTemplate()
    {
        if ( this._collectionBuilderField!.Value == null )
        {
            return this._initialValueField!.Value!;
        }
        else
        {
            return this._collectionBuilderProperty!.Value!.ToImmutable();
        }
    }

    public override IExpression GetBuilderPropertyValue()
        => this._getImmutableValueMethod!.CreateInvokeExpression();

    public override bool TryImportBuilderArtifactsFromBaseType(
        INamedType baseType,
        ScopedDiagnosticSink diagnosticSink )
    {
        // Find the property containing the collection builder.
        if ( !this.TryFindBuilderPropertyInBaseType(
                baseType,
                diagnosticSink,
                out this._collectionBuilderProperty ) )
        {
            return false;
        }

        // Find the method GetImmutable* method.
        this._getImmutableValueMethod =
            baseType.AllMethods.OfName( "GetImmutable" + this.SourceProperty.Name )
                .SingleOrDefault();

        if ( this._getImmutableValueMethod == null )
        {
            diagnosticSink.Report(
                BuilderDiagnosticDefinitions.BaseBuilderMustContainGetImmutableMethod.WithArguments(
                    (baseType, this.SourceProperty.Name) ) );

            return false;
        }

        return true;
    }

    public override void SetBuilderPropertyValue(
        IExpression expression,
        IExpression builderInstance )
    {
        this._initialValueField!.WithObject( builderInstance ).Value = expression.Value;
    }
}