using Metalama.Samples.UniversalInterceptor;
using Metalama.Samples.UniversalInterceptor.Weather;
using Metalama.Samples.UniversalInterceptor2;
internal static class MetalamaInterceptors
{
  internal static async ValueTask WeatherClient_ClearCacheAsync_Interceptor(WeatherClient receiver, CancellationToken cancellationToken)
  {
    var args = (receiver, cancellationToken);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "ClearCacheAsync", "WeatherClient.ClearCacheAsync(CancellationToken)", true);
    IUniversalInterceptor interceptor = CallLog.Current;
    await interceptor.InvokeAsync(args, metadata, ProceedAsync);
    static async ValueTask<ValueTuple> ProceedAsync((WeatherClient receiver, CancellationToken cancellationToken) receivedArgs)
    {
      await receivedArgs.receiver.ClearCacheAsync(receivedArgs.cancellationToken);
      return default;
    }
  }
  internal static string[] WeatherClient_GetAlerts_Interceptor(WeatherClient receiver, ref int cursor, int pageSize)
  {
    var args = (receiver, cursor, pageSize);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetAlerts", "WeatherClient.GetAlerts(ref int, int)", true);
    IUniversalInterceptor interceptor = CallLog.Current;
    try
    {
      return interceptor.Invoke(ref args, metadata, Proceed);
    }
    finally
    {
      cursor = args.cursor;
    }
    static string[] Proceed(ref (WeatherClient receiver, int cursor, int pageSize) receivedArgs)
    {
      return receivedArgs.receiver.GetAlerts(ref receivedArgs.cursor, receivedArgs.pageSize);
    }
  }
  internal static async Task<Forecast> WeatherClient_GetForecastAsync_Interceptor(WeatherClient receiver, string city, int days, CancellationToken cancellationToken)
  {
    var args = (receiver, city, days, cancellationToken);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetForecastAsync", "WeatherClient.GetForecastAsync(string, int, CancellationToken)", true);
    IUniversalInterceptor interceptor = CallLog.Current;
    return await interceptor.InvokeAsync(args, metadata, ProceedAsync);
    static Task<Forecast> ProceedAsync((WeatherClient receiver, string city, int days, CancellationToken cancellationToken) receivedArgs)
    {
      return receivedArgs.receiver.GetForecastAsync(receivedArgs.city, receivedArgs.days, receivedArgs.cancellationToken);
    }
  }
  internal static RequestQuota WeatherClient_GetQuota_Interceptor(WeatherClient receiver)
  {
    var args = ValueTuple.Create(receiver);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetQuota", "WeatherClient.GetQuota()", true);
    IUniversalInterceptor interceptor = CallLog.Current;
    return interceptor.Invoke(ref args, metadata, Proceed);
    static RequestQuota Proceed(ref ValueTuple<WeatherClient> receivedArgs)
    {
      return receivedArgs.Item1.GetQuota();
    }
  }
  internal static async ValueTask<double> WeatherClient_GetTemperatureAsync_Interceptor(WeatherClient receiver, string city, CancellationToken cancellationToken)
  {
    var args = (receiver, city, cancellationToken);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetTemperatureAsync", "WeatherClient.GetTemperatureAsync(string, CancellationToken)", true);
    IUniversalInterceptor interceptor = CallLog.Current;
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
    IUniversalInterceptor interceptor = CallLog.Current;
    await interceptor.InvokeAsync(args, metadata, ProceedAsync);
    static async Task<ValueTuple> ProceedAsync((WeatherClient receiver, CancellationToken cancellationToken) receivedArgs)
    {
      await receivedArgs.receiver.RefreshAsync(receivedArgs.cancellationToken);
      return default;
    }
  }
  internal static bool WeatherClient_TryGetHumidity_Interceptor(WeatherClient receiver, string city, out int humidity)
  {
    var args = (receiver, city, humidity: default(int));
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "TryGetHumidity", "WeatherClient.TryGetHumidity(string, out int)", true);
    IUniversalInterceptor interceptor = CallLog.Current;
    try
    {
      return interceptor.Invoke(ref args, metadata, Proceed);
    }
    finally
    {
      humidity = args.humidity;
    }
    static bool Proceed(ref (WeatherClient receiver, string city, int humidity) receivedArgs)
    {
      return receivedArgs.receiver.TryGetHumidity(receivedArgs.city, out receivedArgs.humidity);
    }
  }
}