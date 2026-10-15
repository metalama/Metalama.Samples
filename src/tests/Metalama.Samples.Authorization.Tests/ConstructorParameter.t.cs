namespace Metalama.Samples.Authorization.Tests.ConstructorParameter;
public class Folder : ISecurable;
public class Document
{
  // Creating a document in a folder requires a permission on the folder.
  public Document([RequiresPermission("AddDocument")] Folder folder)
  {
    ISecurable? argument = folder;
    if (argument != null)
    {
      AccessGuard.Demand(argument, "AddDocument", "Document(folder)", AccessKind.Execute);
    }
    this.Folder = folder;
  }
  public Folder Folder { get; }
}