using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Extensions.Validation;

public class SingletonAttribute : TypeAspect
{
    private static readonly DiagnosticDefinition<(DeclarationKind, IDeclaration, IDeclaration)> _onlyAccessibleFrom
        = new( "SING02", Severity.Warning, "The {0} '{1}' cannot be referenced by {2}." );

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var instanceProperty = builder.Target.Properties.OfName( "Instance" ).SingleOrDefault();

        builder.Outbound
            .SelectMany( type => type.Constructors )
            .ValidateReferences( new ReferenceMemberValidator( instanceProperty ) );
    }

    public class ReferenceMemberValidator : ReferenceValidator
    {
        private readonly IRef<IDeclaration>? _memberRef;

        public override ReferenceKinds ValidatedReferenceKinds => ReferenceKinds.All;

        public ReferenceMemberValidator( IMember? member )
        {
            this._memberRef = member?.ToRef();
        }

        public override void Validate( in ReferenceValidationContext context )
        {
            if ( !context.ReferencingDeclaration.Equals( this._memberRef?.GetTarget( options: default ) ) )
            {
                context.Diagnostics.Report(
                    _onlyAccessibleFrom.WithArguments(
                        (context.ReferencedDeclaration.DeclarationKind, context.ReferencedDeclaration, context.ReferencingDeclaration) ) );
            }
        }
    }
}