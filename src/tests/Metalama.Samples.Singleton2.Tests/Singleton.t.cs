// Warning LAMA0905 on `new ConfigurationManager( configurationSource )`: `The 'ConfigurationManager.ConfigurationManager(IConfigurationSource)' constructor cannot be referenced by the 'ProductionClass' type. The class is a [Singleton].`
using System.Collections.Frozen;
public interface IConfigurationSource
{
  FrozenDictionary<string, string> LoadConfiguration();
}
[Singleton]
public sealed class ConfigurationManager
{
  private readonly FrozenDictionary<string, string> _dictionary;
  public ConfigurationManager( IConfigurationSource configurationSource )
  {
    this._dictionary = configurationSource.LoadConfiguration();
  }
  public string GetValue( string key ) => this._dictionary[key];
}
namespace Prod
{
  internal class ProductionClass
  {
    private void M( IConfigurationSource configurationSource ) => _ = new ConfigurationManager( configurationSource );
  }
}
namespace Tests
{
  internal class TestClass
  {
    private void M( IConfigurationSource configurationSource ) => _ = new ConfigurationManager( configurationSource );
  }
}