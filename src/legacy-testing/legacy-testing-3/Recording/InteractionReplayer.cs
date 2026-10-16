using System.Text.Json;

namespace Metalama.Samples.LegacyTesting3.Recording;

// Returns the recorded results instead of calling the dependencies. It checks that the legacy code makes the
// recorded calls, in the same order and with the same arguments.
public static class InteractionReplayer
{
    private static readonly Queue<Interaction> _pendingInteractions = new();

    public static int PendingInteractionCount => _pendingInteractions.Count;

    public static void Load( string json )
    {
        _pendingInteractions.Clear();

        foreach ( var interaction in JsonSerializer.Deserialize<List<Interaction>>( json ) ?? [] )
        {
            _pendingInteractions.Enqueue( interaction );
        }
    }

    // Replays a call to a method that returns void.
    public static void Replay( string method, IReadOnlyList<object?> arguments ) => Dequeue( method, arguments );

    // Replays a call to a method that returns a value, and returns the recorded result.
    public static object? Replay( string method, IReadOnlyList<object?> arguments, Type resultType )
        => Dequeue( method, arguments ).Result?.Deserialize( resultType );

    private static Interaction Dequeue( string method, IReadOnlyList<object?> arguments )
    {
        if ( !_pendingInteractions.TryDequeue( out var interaction ) )
        {
            throw new InvalidOperationException(
                $"The code calls {method}, but the recording has no more interactions." );
        }

        if ( interaction.Method != method )
        {
            throw new InvalidOperationException(
                $"The code calls {method}, but the recording expects a call to {interaction.Method}." );
        }

        for ( var i = 0; i < arguments.Count; i++ )
        {
            var actualArgument = JsonSerializer.SerializeToElement( arguments[i] );

            if ( !JsonElement.DeepEquals( actualArgument, interaction.Arguments[i] ) )
            {
                throw new InvalidOperationException(
                    $"The code calls {method} with the argument {actualArgument.GetRawText()}, "
                    + $"but the recording has {interaction.Arguments[i].GetRawText()}." );
            }
        }

        return interaction;
    }
}
