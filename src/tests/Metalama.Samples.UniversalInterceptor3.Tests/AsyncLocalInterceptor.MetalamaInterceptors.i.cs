using Metalama.Samples.UniversalInterceptor;
using Metalama.Samples.UniversalInterceptor.Weather;
using Metalama.Samples.UniversalInterceptor3;
internal static class MetalamaInterceptors
{
  internal static async Task<Forecast> WeatherClient_GetForecastAsync_Interceptor(WeatherClient receiver, string city, int days, CancellationToken cancellationToken)
  {
    var args = (receiver, city, days, cancellationToken);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetForecastAsync", "WeatherClient.GetForecastAsync(string, int, CancellationToken)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
    return await interceptor.InvokeAsync(args, metadata, ProceedAsync);
    static Task<Forecast> ProceedAsync((WeatherClient receiver, string city, int days, CancellationToken cancellationToken) receivedArgs)
    {
      return receivedArgs.receiver.GetForecastAsync(receivedArgs.city, receivedArgs.days, receivedArgs.cancellationToken);
    }
  }
  internal static double WeatherClient_GetTemperature_Interceptor(WeatherClient receiver, string city)
  {
    var args = (receiver, city);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetTemperature", "WeatherClient.GetTemperature(string)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
    return (double)interceptor.Invoke(ref args, metadata, Proceed);
    static double Proceed(ref (WeatherClient receiver, string city) receivedArgs)
    {
      return receivedArgs.receiver.GetTemperature(receivedArgs.city);
    }
  }
}