using Metalama.Samples.UniversalInterceptor;
using Xunit;

namespace Metalama.Samples.UniversalInterceptor3.UnitTests;

internal static class InterceptorScope
{
    // Runs the code of a test in a new asynchronous flow, in which CallLog.Current gives the interceptor. The value
    // assigned in this flow is not visible in the flow of the test runner, or in the flows of the other tests, so the
    // tests that run in parallel can install different interceptors.
    public static Task RunAsync( IUniversalInterceptor interceptor, Func<Task> test )
        => Task.Run(
            async () =>
            {
                CallLog.Current.Value = interceptor;
                await test();
            },
            TestContext.Current.CancellationToken );
}
