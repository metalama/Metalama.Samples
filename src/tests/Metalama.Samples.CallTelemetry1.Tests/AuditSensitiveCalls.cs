#if TEST_OPTIONS
// @Include(../../call-telemetry/call-telemetry-1/AuditFabric.cs)
// @Include(../../call-telemetry/call-telemetry-1/Audit.cs)
#endif

using System.Diagnostics;

namespace Metalama.Samples.CallTelemetry1.Tests.AuditSensitiveCalls;

public class BackupService
{
    public void Archive( string folder, string archive )
    {
        using var tar = Process.Start( "tar", $"-cf \"{archive}\" -C \"{folder}\" ." );
        tar.WaitForExit();
    }

    public void ArchiveWithStartInfo( string folder, string archive )
    {
        using var tar = Process.Start( new ProcessStartInfo( "tar", ["-cf", archive, "-C", folder, "."] ) );
        tar?.WaitForExit();
    }

    public void Clean( string folder )
    {
        foreach ( var file in Directory.GetFiles( folder ) )
        {
            File.Delete( file );
        }
    }
}
