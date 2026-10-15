namespace Metalama.Samples.Authorization.Tests.SecurableInterface;

// Implementing ISecurable is enough: the class inherits the SecurableType aspect from the interface,
// so its members are secured by default and validated.
public class Document : ISecurable
{
    public string? Title { get; set; }

    public void Publish() { }
}

// The aspect is also inherited through a derived interface, and by derived classes.
public interface IArchive : ISecurable;

public class Archive : IArchive
{
    public int Capacity { get; set; }
}

public class ColdArchive : Archive
{
    public string? Location { get; set; }
}
