internal class IllegalUse
{
    public void SomeMethod()
        =>

            // This call is illegal and reported.
            _ = new PerformanceCounterManager( new AwsPerformanceCounterUploader() );
}