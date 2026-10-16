namespace Metalama.Samples.Authorization.Tests.GrantsPermissions_Eligibility;

public class Report : ISecurable
{
    public string? Body { get; set; }

    // An automatic property has no code that could use the grant.
    [RequiresPermission( "Read" )]
    [GrantsPermissions( "Read" )]
    public string? Title { get; set; }

    // The body of an iterator runs after the member has returned, outside of the grant.
    [RequiresPermission( "Read" )]
    [GrantsPermissions( "Read" )]
    public IEnumerable<string> Words()
    {
        foreach ( var word in this.Body?.Split( ' ' ) ?? [] )
        {
            yield return word;
        }
    }
}

// On a parameter, the grant wraps the body of a method, so it cannot be applied to a constructor, and the
// parameter must be securable.
public class Folder : ISecurable
{
    public Folder( [RequiresPermission( "Read" )] [GrantsPermissions( "Read" )] Report report ) { }

    [RequiresPermission( "Read" )]
    public void Rename( [GrantsPermissions( "Read" )] string name ) { }
}

// A grant is a permission on a securable object.
public class Calculator
{
    [GrantsPermissions( "Read" )]
    public int Add( int a, int b ) => a + b;
}
