using Metalama.Samples.UniversalInterceptor;
using Metalama.Samples.UniversalInterceptor.Weather;
using Metalama.Samples.UniversalInterceptor1;
internal static class MetalamaInterceptors
{
  internal static double WeatherClient_GetTemperature_Interceptor(WeatherClient receiver, string city)
  {
    var args = (receiver, city);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetTemperature", "WeatherClient.GetTemperature(string)", true);
    IUniversalInterceptor interceptor = CallLog.Current;
    return (double)interceptor.Invoke(ref args, metadata, Proceed);
    static double Proceed(ref (WeatherClient receiver, string city) receivedArgs)
    {
      return receivedArgs.receiver.GetTemperature(receivedArgs.city);
    }
  }
  internal static void WeatherClient_SetUnit_Interceptor(WeatherClient receiver, TemperatureUnit unit)
  {
    var args = (receiver, unit);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "SetUnit", "WeatherClient.SetUnit(TemperatureUnit)", true);
    IUniversalInterceptor interceptor = CallLog.Current;
    interceptor.Invoke(ref args, metadata, Proceed);
    static ValueTuple Proceed(ref (WeatherClient receiver, TemperatureUnit unit) receivedArgs)
    {
      receivedArgs.receiver.SetUnit(receivedArgs.unit);
      return default;
    }
  }
}