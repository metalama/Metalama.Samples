namespace Tests;

internal class TestPerformanceCounterUploader : IPerformanceCounterUploader
{
    public List<KeyValuePair<string, double>> Records { get; } = new();

    public void UploadCounter( string name, double value )
        => this.Records.Add( new KeyValuePair<string, double>( name, value ) );
}