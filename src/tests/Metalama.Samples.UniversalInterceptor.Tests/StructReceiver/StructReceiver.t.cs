using Metalama.Samples.UniversalInterceptor.Weather;
namespace Metalama.Samples.UniversalInterceptor.Tests.StructReceiver;
// The fabric skips the call on the RequestQuota struct with ForEachSite, so the call is not intercepted, and no warning
// is reported. The call to GetQuota, on a WeatherClient, is intercepted.
public class WeatherReport
{
  private readonly WeatherClient _client = new();
  public bool TrySendRequest()
  {
    var quota = MetalamaInterceptors.WeatherClient_GetQuota_Interceptor(this._client);
    return quota.TryConsume();
  }
}