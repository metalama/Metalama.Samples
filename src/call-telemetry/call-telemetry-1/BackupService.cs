using System.Diagnostics;

namespace Metalama.Samples.CallTelemetry1;

public class BackupService
{
    // Archives the files of a folder with the tar tool, then deletes them.
    public void Backup( string folder, string archive )
    {
        using var tar = Process.Start( new ProcessStartInfo( "tar", ["-cf", archive, "-C", folder, "."] ) );
        tar?.WaitForExit();

        foreach ( var file in Directory.GetFiles( folder ) )
        {
            File.Delete( file );
        }
    }
}
