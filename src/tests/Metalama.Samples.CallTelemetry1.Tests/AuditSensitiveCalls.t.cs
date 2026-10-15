using System.Diagnostics;
namespace Metalama.Samples.CallTelemetry1.Tests.AuditSensitiveCalls;
public class BackupService
{
  public void Archive(string folder, string archive)
  {
    using var tar = Audit.StartProcess("tar", $"-cf \"{archive}\" -C \"{folder}\" .", caller: "Archive", file: "AuditSensitiveCalls.cs", line: 10);
    tar.WaitForExit();
  }
  public void ArchiveWithStartInfo(string folder, string archive)
  {
    using var tar = Audit.StartProcess(new ProcessStartInfo("tar", ["-cf", archive, "-C", folder, "."]), caller: "ArchiveWithStartInfo", file: "AuditSensitiveCalls.cs", line: 16);
    tar?.WaitForExit();
  }
  public void Clean(string folder)
  {
    foreach (var file in Directory.GetFiles(folder))
    {
      Audit.DeleteFile(file, caller: "Clean", file: "AuditSensitiveCalls.cs", line: 24);
    }
  }
}