using Metalama.Framework.Aspects;

namespace Metalama.Samples.NormalizeStrings;

[RunTimeOrCompileTime]
public sealed class NormalizeAttribute : StringContractAspect
{
    public override void Validate( dynamic? value )
    {
        if ( IsAppliedToNullableString() )
        {
            value = ((string?) value)?.Normalize();
        }
        else
        {
            value = ((string) value!).Normalize();
        }
    }
}