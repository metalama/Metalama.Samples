using Xunit;

namespace Tests;

internal class TestClass
{
    [Fact]
    public void M()
    {
        var testUploader = new TestPerformanceCounterUploader();

        // This instantiation of PerformanceCounterManager is allowed because we are in a test namespace.
        var manager = new PerformanceCounterManager( testUploader );
        manager.IncrementCounter( "Foo" );
        manager.UploadAndReset();

        Assert.Single( testUploader.Records );
    }
}