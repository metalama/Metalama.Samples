using Metalama.Samples.UniversalInterceptor.Weather;
using Xunit;

namespace Metalama.Samples.UniversalInterceptor.UnitTests;

// xUnit runs the test classes in parallel. Each test installs its own fake interceptor.
public class WeatherReportTests
{
    [Fact]
    public Task Describe_UsesTheCannedTemperature()
    {
        // The weather client does not know Oslo, so the result comes from the fake. SetUnit has no canned result, so
        // the call proceeds to the weather client.
        var fake = new FakeWeatherInterceptor()
            .Returns( nameof(WeatherClient.GetTemperature), -3.5 );

        return InterceptorScope.RunAsync(
            fake,
            () =>
            {
                Assert.Equal( "Oslo: -3.5 Celsius", new WeatherReport().Describe( "Oslo" ) );
                Assert.Equal( [nameof(WeatherClient.SetUnit), nameof(WeatherClient.GetTemperature)], fake.Calls );

                return Task.CompletedTask;
            } );
    }

    [Fact]
    public Task Describe_HandlesAnUnknownCity()
    {
        // The fake throws for Prague, which the weather client knows.
        var fake = new FakeWeatherInterceptor()
            .Throws( nameof(WeatherClient.GetTemperature), new ArgumentException( "Unknown city." ) );

        return InterceptorScope.RunAsync(
            fake,
            () =>
            {
                Assert.Equal( "Prague: unknown city", new WeatherReport().Describe( "Prague" ) );

                return Task.CompletedTask;
            } );
    }

    [Fact]
    public Task DescribeHumidity_UsesTheCannedResult()
    {
        // The weather client knows the humidity of Prague, but the fake returns false.
        var fake = new FakeWeatherInterceptor()
            .Returns( nameof(WeatherClient.TryGetHumidity), false );

        return InterceptorScope.RunAsync(
            fake,
            () =>
            {
                Assert.Equal( "Prague: humidity unknown", new WeatherReport().DescribeHumidity( "Prague" ) );

                return Task.CompletedTask;
            } );
    }
}

public class ForecastTests
{
    [Fact]
    public Task DescribeForecastAsync_UsesTheCannedResults()
    {
        var fake = new FakeWeatherInterceptor()
            .Returns( nameof(WeatherClient.GetForecastAsync), new Forecast( "Oslo", 2, 1.5 ) )
            .Returns( nameof(WeatherClient.GetTemperatureAsync), 0.5 );

        return InterceptorScope.RunAsync(
            fake,
            async () => Assert.Equal(
                "Oslo: 0.5 now, 1.5 on average in the next 2 days",
                await new WeatherReport().DescribeForecastAsync( "Oslo", 2 ) ) );
    }

    [Fact]
    public Task DescribeForecastAsync_ProceedsWithoutACannedResult()
    {
        // The fake has no canned result, so the calls proceed to the weather client.
        var fake = new FakeWeatherInterceptor();

        return InterceptorScope.RunAsync(
            fake,
            async () =>
            {
                Assert.Equal(
                    "Lisbon: 21 now, 22 on average in the next 4 days",
                    await new WeatherReport().DescribeForecastAsync( "Lisbon", 4 ) );

                Assert.Equal(
                    [
                        nameof(WeatherClient.RefreshAsync),
                        nameof(WeatherClient.ClearCacheAsync),
                        nameof(WeatherClient.GetForecastAsync),
                        nameof(WeatherClient.GetTemperatureAsync)
                    ],
                    fake.Calls );
            } );
    }
}

public class IsolationTests
{
    // Two asynchronous flows run at the same time with different interceptors. Each flow waits until the other flow
    // has installed its interceptor before it calls the weather client, so the two interceptors are installed at the
    // same time, and each call still reaches the interceptor of its own flow.
    [Fact]
    public async Task ConcurrentFlows_UseTheirOwnInterceptor()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var firstInstalled = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var secondInstalled = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );

        var first = RunFlowAsync( 10.0, firstInstalled, secondInstalled.Task );
        var second = RunFlowAsync( 20.0, secondInstalled, firstInstalled.Task );

        var results = await Task.WhenAll( first, second );

        Assert.Equal( "Oslo: 10 Celsius", results[0] );
        Assert.Equal( "Oslo: 20 Celsius", results[1] );

        Task<string> RunFlowAsync( double temperature, TaskCompletionSource installed, Task otherInstalled )
            => Task.Run(
                async () =>
                {
                    CallLog.Current.Value = new FakeWeatherInterceptor()
                        .Returns( nameof(WeatherClient.GetTemperature), temperature );

                    installed.SetResult();
                    await otherInstalled.WaitAsync( cancellationToken );

                    return new WeatherReport().Describe( "Oslo" );
                },
                cancellationToken );
    }
}
