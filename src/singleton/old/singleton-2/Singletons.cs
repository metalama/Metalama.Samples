[Singleton]
internal class PrivateConstructorSingleton
{
    private PrivateConstructorSingleton() { }

    public PrivateConstructorSingleton Instance { get; } = new();
}

[Singleton]
internal class PublicConstructorSingleton
{
    public PublicConstructorSingleton() { }

    public PublicConstructorSingleton Instance { get; } = new();
}