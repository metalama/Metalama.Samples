namespace Metalama.Samples.Proxy;

public interface ICalculator
{
    int Add( int a, int b );

    void Store( string name, int value );

    void Clear();
}
