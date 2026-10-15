using Metalama.Samples.UniversalInterceptor;
using Metalama.Samples.UniversalInterceptor.Weather;
internal static class MetalamaInterceptors
{
  internal static RequestQuota WeatherClient_GetQuota_Interceptor(WeatherClient receiver)
  {
    var args = ValueTuple.Create(receiver);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetQuota", "WeatherClient.GetQuota()", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
    return interceptor.Invoke(ref args, metadata, Proceed);
    static RequestQuota Proceed(ref ValueTuple<WeatherClient> receivedArgs)
    {
      return receivedArgs.Item1.GetQuota();
    }
  }
}