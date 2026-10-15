using Metalama.Samples.UniversalInterceptor;
using Metalama.Samples.UniversalInterceptor.Weather;
internal static class MetalamaInterceptors
{
  internal static string[] WeatherClient_GetAlerts_Interceptor(WeatherClient receiver, ref int cursor, int pageSize)
  {
    var args = (receiver, cursor, pageSize);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetAlerts", "WeatherClient.GetAlerts(ref int, int)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
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
  internal static bool WeatherClient_TryGetHumidity_Interceptor(WeatherClient receiver, string city, out int humidity)
  {
    var args = (receiver, city, humidity: default(int));
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "TryGetHumidity", "WeatherClient.TryGetHumidity(string, out int)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
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