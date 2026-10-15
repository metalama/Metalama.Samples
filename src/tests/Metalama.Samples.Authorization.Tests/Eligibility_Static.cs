namespace Metalama.Samples.Authorization.Tests.Eligibility_Static;

public class Registry
{
    // Static members have no instance on which to check the permission.
    [RequiresPermission( "Write" )]
    public static void Reset() { }

    [RequiresPermission( "Read" )]
    public static int Count { get; set; }
}
