using Metalama.Framework.Code;

namespace Metalama.Samples.Comparison4;

public class DateEqualityMemberAttribute : EqualityMemberAttribute
{
    public DateEqualityMemberAttribute( int order = DefaultOrder ) : base( order ) { }

    protected internal override IExpression GetComparerExpression( IFieldOrProperty field )
        => TypeFactory.GetNamedType( typeof(DateComparer) ).Properties[nameof(DateComparer.Instance)];
}