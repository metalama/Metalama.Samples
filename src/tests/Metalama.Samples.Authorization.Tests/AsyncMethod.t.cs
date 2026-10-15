namespace Metalama.Samples.Authorization.Tests.AsyncMethod;
public class Document : ISecurable
{
  // The check runs at the start of the async method, so a denial faults the returned task.
  [RequiresPermission("Send")]
  public async Task SendAsync(CancellationToken cancellationToken = default)
  {
    AccessGuard.Demand(this, "Send", "Document.SendAsync", AccessKind.Execute);
    await Task.Delay(1, cancellationToken);
  }
}