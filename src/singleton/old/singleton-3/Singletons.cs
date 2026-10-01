[Singleton]
public class MySingleton
{
    public MySingleton() { }

    public MySingleton Instance { get; } = new();
}

namespace Prod
{
    internal class ProductionClass
    {
        public void M()
        {
            _ = new MySingleton();
        }
    }
}

namespace Tests
{
    internal class TestClass
    {
        public void M()
        {
            _ = new MySingleton();
        }
    }
}