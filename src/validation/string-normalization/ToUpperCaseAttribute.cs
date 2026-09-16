using Metalama.Framework.Aspects;
using System.Globalization;

namespace Metalama.Samples.NormalizeStrings;

[RunTimeOrCompileTime]
public sealed class ToUpperCaseAttribute : StringContractAspect
{
    public override void Validate( dynamic? value )
    {
        if ( IsAppliedToNullableString() )
        {
            value = ((string?) value)?.ToUpper( CultureInfo.CurrentCulture );
        }
        else
        {
            value = ((string) value!).ToUpper( CultureInfo.CurrentCulture );
        }
    }
}