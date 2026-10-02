using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

internal class IgnoreValuesAttribute : OverrideFieldOrPropertyAspect
{
    // [<snippet Constructor>]
    private readonly object?[] _ignoredValues;

    public IgnoreValuesAttribute( params object?[] values )
    {
        this._ignoredValues = values;
    }

    // [<endsnippet Constructor>]

    public override dynamic? OverrideProperty
    {
        get => meta.Proceed();
        set
        {
            foreach ( var ignoredValue in this._ignoredValues )
            {
                if ( value == meta.RunTime( ignoredValue ) )
                {
                    return;
                }
            }

            meta.Proceed();
        }
    }

    public override void BuildEligibility( IEligibilityBuilder<IFieldOrProperty> builder )
    {
        var supportedTypes =
            new[]
            {
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(short),
                typeof(sbyte),
                typeof(byte),
                typeof(ushort),
                typeof(char),
                typeof(string),
                typeof(bool),
                typeof(Type)
            };

        builder.Type()
            .MustSatisfyAny(
                supportedTypes.Select( supportedType =>
                                           new Action<IEligibilityBuilder<IType>>( t => t.MustEqual( supportedType ) ) )
                    .ToArray() );
    }
}