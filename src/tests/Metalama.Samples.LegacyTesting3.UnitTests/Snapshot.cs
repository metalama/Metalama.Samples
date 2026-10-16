using System.Reflection;
using Xunit;

namespace Metalama.Samples.LegacyTesting3.UnitTests;

// Compares a value with a file of the Snapshots directory. When they differ, the test fails, and the value is written
// to a .received.txt file next to the .verified.txt file. To accept the new value, replace the .verified.txt file with
// the .received.txt file.
internal static class Snapshot
{
    // The directory of the project is stored in the assembly by the project file. [CallerFilePath] cannot be used
    // because a deterministic build maps the source paths to '/_/'.
    private static readonly string _directory = Path.Combine(
        typeof(Snapshot).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single( a => a.Key == "ProjectDirectory" ).Value!,
        "Snapshots" );

    public static void Verify( string actual, string name )
    {
        var verifiedPath = Path.Combine( _directory, $"{name}.verified.txt" );
        var receivedPath = Path.Combine( _directory, $"{name}.received.txt" );

        // Line endings depend on the platform and on the Git settings, so they are not compared.
        actual = NormalizeLineEndings( actual );
        var verified = File.Exists( verifiedPath ) ? NormalizeLineEndings( File.ReadAllText( verifiedPath ) ) : null;

        if ( actual == verified )
        {
            File.Delete( receivedPath );

            return;
        }

        File.WriteAllText( receivedPath, actual );

        Assert.Fail( $"The value differs from the snapshot '{verifiedPath}'. The value is in '{receivedPath}'." );
    }

    private static string NormalizeLineEndings( string text ) => text.ReplaceLineEndings( "\n" );
}
