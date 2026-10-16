using System.Text.Json;

namespace Metalama.Samples.LegacyTesting2.Recording;

// A call of the legacy code to a dependency: the called method, the arguments, and the result.
public record Interaction( string Method, IReadOnlyList<JsonElement> Arguments, JsonElement? Result );
