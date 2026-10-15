using Metalama.Extensions.Interceptors;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Metalama.Samples.CallTelemetry1;

// Records the calls to sensitive APIs, then performs them. The calls in this class are not intercepted,
// so that the audit methods call the original APIs.
[ExcludeInterceptors]
public static class Audit
{
    public static void DeleteFile(
        string path,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0 )
    {
        Write( caller, file, line, $"File.Delete {Path.GetFileName( path )}" );

        File.Delete( path );
    }

    public static Process StartProcess(
        string fileName,
        string arguments,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0 )
    {
        Write( caller, file, line, $"Process.Start {fileName} {arguments}" );

        return Process.Start( fileName, arguments );
    }

    public static Process? StartProcess(
        ProcessStartInfo startInfo,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0 )
    {
        Write( caller, file, line, $"Process.Start {startInfo.FileName} {string.Join( ' ', startInfo.ArgumentList )}" );

        return Process.Start( startInfo );
    }

    private static void Write( string caller, string file, int line, string operation )
        => Console.WriteLine( $"[audit] {Path.GetFileName( file )}, line {line}, {caller}: {operation}" );
}
