using Metalama.Framework.Code;

namespace Metalama.Samples.Comparison4;

public partial class StringEqualityMemberAttribute
{
    internal override int GetCost( IFieldOrProperty field )
    {
        // TODO: Use benchmark to determine relative costs.

        var cost = this._stringComparison switch
        {
            StringComparison.Ordinal => 5,
            StringComparison.OrdinalIgnoreCase => 10,
            _ => 50
        };

        if ( this._trim )
        {
            cost += 20;
        }

        return cost;
    }
}