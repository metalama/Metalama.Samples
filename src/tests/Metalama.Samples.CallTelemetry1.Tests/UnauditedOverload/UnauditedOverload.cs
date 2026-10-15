#if TEST_OPTIONS
// @Include(../../../call-telemetry/call-telemetry-1/AuditFabric.cs)
// @Include(../../../call-telemetry/call-telemetry-1/Audit.cs)
#endif

using System.Diagnostics;

namespace Metalama.Samples.CallTelemetry1.Tests.UnauditedOverload;

public class Launcher
{
    // The Audit class has no method for Process.Start( string ), so the fabric reports an error.
    public void Open( string fileName ) => Process.Start( fileName );
}
