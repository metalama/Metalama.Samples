// Warning UNIV05 on `TryConsume`: `The call to 'RequestQuota.TryConsume()' is not intercepted because its receiver is a struct, and the argument tuple would contain a copy of the receiver. To remove this warning, skip the call site in ForEachSite.`
public class Sender
{
  public bool TrySend(RequestQuota quota) => quota.TryConsume();
}