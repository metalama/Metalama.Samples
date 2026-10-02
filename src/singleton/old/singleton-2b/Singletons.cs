[Singleton]
internal class CorrectSingleton
{
    private CorrectSingleton() { }

    public CorrectSingleton Instance { get; } = new();
}

[Singleton]
internal class DifferentlyNamedPropertySingleton
{
    private DifferentlyNamedPropertySingleton() { }

    public DifferentlyNamedPropertySingleton Value { get; } = new();
}

[Singleton]
internal class MethodInsteadOfPropertySingleton
{
    private MethodInsteadOfPropertySingleton() { }

    public MethodInsteadOfPropertySingleton GetInstance() => new();
}

[Singleton]
internal class CallingConstructorFromGetterSingleton
{
    private CallingConstructorFromGetterSingleton() { }

    public CallingConstructorFromGetterSingleton Instance
    {
        get => new();
    }
}