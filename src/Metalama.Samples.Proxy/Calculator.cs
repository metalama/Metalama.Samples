namespace Metalama.Samples.Proxy;

public sealed class Calculator : ICalculator
{
    private readonly Dictionary<string, int> _memory = [];

    public IReadOnlyDictionary<string, int> Memory => this._memory;

    public int Add( int a, int b ) => a + b;

    public void Store( string name, int value ) => this._memory[name] = value;

    public void Clear() => this._memory.Clear();
}
