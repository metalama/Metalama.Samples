internal class AwsPerformanceCounterUploader : IPerformanceCounterUploader
{
    public void UploadCounter( string name, double value ) => Console.WriteLine( $"Uploading.... {name} -> {value}" );
}