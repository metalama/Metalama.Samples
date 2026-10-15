using Metalama.Extensions.Interceptors;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;
using System.Diagnostics;

namespace Metalama.Samples.CallTelemetry1;

// Redirects the calls to sensitive APIs to the Audit class, in the whole project.
internal class AuditFabric : ProjectFabric
{
    private static readonly DiagnosticDefinition<IMethod> _callNotAudited = new(
        "AUDIT01",
        Severity.Error,
        "The call to '{0}' cannot be audited because the Audit class has no method for this overload." );

    public override void AmendProject( IProjectAmender amender )
    {
        // File.Delete has a single overload, and Audit has a single DeleteFile method.
        var deleteFile = typeof(Audit).AsINamedType().Methods.OfName( nameof(Audit.DeleteFile) ).Single();

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(File) )
                .Methods( nameof(File.Delete) )
                .RedirectToExistingMethod( deleteFile ) );

        // Process.Start has several overloads. Each call site is redirected to the StartProcess method that has the
        // same parameters. A call to an overload without such a method is an error, so that no call escapes the audit.
        amender.InterceptMethods(
            calls => calls
                .Type( typeof(Process) )
                .Methods( nameof(Process.Start) )
                .ForEachSite(
                    site =>
                    {
                        var startProcess = FindAuditMethod( nameof(Audit.StartProcess), site.InterceptedMethod );

                        if ( startProcess == null )
                        {
                            site.Diagnostics.Report( _callNotAudited.WithArguments( site.InterceptedMethod ) );

                            return site.Skip();
                        }

                        return site.RedirectToExistingMethod( startProcess );
                    } ) );
    }

    // Finds the audit method that has a parameter of the same name and type for each parameter of the static
    // intercepted method, and whose other parameters are optional, like the caller information parameters.
    private static IMethod? FindAuditMethod( string name, IMethod interceptedMethod )
    {
        if ( !interceptedMethod.IsStatic )
        {
            return null;
        }

        return typeof(Audit).AsINamedType()
            .Methods.OfName( name )
            .SingleOrDefault(
                auditMethod => interceptedMethod.Parameters.All(
                                   p => auditMethod.Parameters.Any( a => a.Name == p.Name && a.Type.Equals( p.Type ) ) )
                               && auditMethod.Parameters.All(
                                   a => interceptedMethod.Parameters.Any( p => p.Name == a.Name )
                                        || a.DefaultValue != null ) );
    }
}
