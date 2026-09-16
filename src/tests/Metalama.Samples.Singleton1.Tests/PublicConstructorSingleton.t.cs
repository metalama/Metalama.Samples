// Warning SING01 on `PublicConstructorSingleton`: `The 'PublicConstructorSingleton.PublicConstructorSingleton()' constructor must be private because the class is [Singleton].`
[Singleton]
internal class PublicConstructorSingleton
{
  public PublicConstructorSingleton()
  {
  }
  public static PublicConstructorSingleton Instance { get; } = new global::PublicConstructorSingleton();
}