[Singleton]
internal class PrivateConstructorSingleton
{
  private PrivateConstructorSingleton()
  {
  }
  public static PrivateConstructorSingleton Instance { get; } = new global::PrivateConstructorSingleton();
}