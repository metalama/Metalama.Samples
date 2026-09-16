using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Documentation.QuickStart;

public class NoNullStringAttribute : OverrideFieldOrPropertyAspect
{
    public override dynamic? OverrideProperty
    {
        get
        {
            if ( meta.Target.FieldOrProperty.Value == null )
            {
                if ( meta.Target.FieldOrProperty.Type.Equals( SpecialType.String ) )
                {
                    meta.Target.FieldOrProperty.Value = string.Empty;
                }
            }

            return meta.Proceed();
        }
        set => meta.Proceed();
    }
}