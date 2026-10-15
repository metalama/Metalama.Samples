using Metalama.Samples.UniversalInterceptor;
using Metalama.Samples.UniversalInterceptor.Weather;
internal static class MetalamaInterceptors
{
  internal static async ValueTask WeatherClient_ClearCacheAsync_Interceptor(WeatherClient receiver, CancellationToken cancellationToken)
  {
    var args = (receiver, cancellationToken);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "ClearCacheAsync", "WeatherClient.ClearCacheAsync(CancellationToken)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
    await interceptor.InvokeAsync(args, metadata, ProceedAsync);
    static async ValueTask<ValueTuple> ProceedAsync((WeatherClient receiver, CancellationToken cancellationToken) receivedArgs)
    {
      await receivedArgs.receiver.ClearCacheAsync(receivedArgs.cancellationToken);
      return default;
    }
  }
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
  internal static async ValueTask<double> WeatherClient_GetTemperatureAsync_Interceptor(WeatherClient receiver, string city, CancellationToken cancellationToken)
  {
    var args = (receiver, city, cancellationToken);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetTemperatureAsync", "WeatherClient.GetTemperatureAsync(string, CancellationToken)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
    return (double)await interceptor.InvokeAsync(args, metadata, ProceedAsync);
    static ValueTask<double> ProceedAsync((WeatherClient receiver, string city, CancellationToken cancellationToken) receivedArgs)
    {
      return receivedArgs.receiver.GetTemperatureAsync(receivedArgs.city, receivedArgs.cancellationToken);
    }
  }
  internal static async Task WeatherClient_RefreshAsync_Interceptor(WeatherClient receiver, CancellationToken cancellationToken)
  {
    var args = (receiver, cancellationToken);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "RefreshAsync", "WeatherClient.RefreshAsync(CancellationToken)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
    await interceptor.InvokeAsync(args, metadata, ProceedAsync);
    static async Task<ValueTuple> ProceedAsync((WeatherClient receiver, CancellationToken cancellationToken) receivedArgs)
    {
      await receivedArgs.receiver.RefreshAsync(receivedArgs.cancellationToken);
      return default;
    }
  }
}