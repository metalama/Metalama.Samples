using System.Text.Json;

namespace Metalama.Samples.LegacyTesting2.Recording;

// Stores the interactions in memory, and writes them to a JSON file when the process exits.
public static class InteractionRecorder
{
    public const string FileName = "interactions.json";

    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    private static readonly List<Interaction> _interactions = new();

    static InteractionRecorder()
    {
        AppDomain.CurrentDomain.ProcessExit += ( _, _ ) => Save( FileName );
    }

    public static void Record( string method, IReadOnlyList<object?> arguments, object? result )
    {
        var interaction = new Interaction(
            method,
            arguments.Select( argument => JsonSerializer.SerializeToElement( argument ) ).ToList(),
            result == null ? null : JsonSerializer.SerializeToElement( result ) );

        lock ( _interactions )
        {
            _interactions.Add( interaction );
        }
    }

    public static void Save( string path )
    {
        lock ( _interactions )
        {
            File.WriteAllText( path, JsonSerializer.Serialize( _interactions, _options ) );
        }
    }
}
