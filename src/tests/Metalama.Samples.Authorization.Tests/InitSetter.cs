namespace Metalama.Samples.Authorization.Tests.InitSetter;

public class Document
{
    // An init accessor is checked: an object initializer written by the caller is a write.
    [RequiresPermission( "Read", "Create" )]
    public string? Title { get; init; }
}
