using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Samples.Comparison3;

public class EqualityMemberAttribute : FieldOrPropertyAspect
{
    public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
    {
        base.BuildAspect( builder );

        // Automatically (and implicitly) add the ImplementEquatableAttribute aspect to the declaring type.
        builder.With( builder.Target.DeclaringType ).RequireAspect<ImplementEquatableAttribute>();
    }

    public override void BuildEligibility( IEligibilityBuilder<IFieldOrProperty> builder )
    {
        base.BuildEligibility( builder );

        builder.MustNotBeStatic();
        builder.MustBeExplicitlyDeclared();
    }
}