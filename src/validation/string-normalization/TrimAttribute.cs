using Metalama.Framework.Aspects;

namespace Metalama.Samples.NormalizeStrings;

[RunTimeOrCompileTime]
public sealed class TrimAttribute : StringContractAspect
{
    public override void Validate( dynamic? value )
    {
        if ( IsAppliedToNullableString() )
        {
            value = ((string?) value)?.Trim();
        }
        else
        {
            value = ((string) value!).Trim();
        }
    }
}