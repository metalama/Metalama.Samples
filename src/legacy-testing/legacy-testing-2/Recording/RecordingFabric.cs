using Metalama.Extensions.Interceptors;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using System.Net.Mail;

namespace Metalama.Samples.LegacyTesting2.Recording;

// Records the calls to the hard-wired dependencies of the legacy code.
internal class RecordingFabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        // The project defines the RECORD_INTERACTIONS symbol only in the Record configuration.
        if ( !amender.Project.PreprocessorSymbols.Contains( "RECORD_INTERACTIONS" ) )
        {
            return;
        }

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(Thread) )
                .Methods( nameof(Thread.Sleep) )
                .RedirectToSynthesizedMethod( nameof(this.Record) ) );

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(File) )
                .Methods( nameof(File.ReadAllText) )
                .RedirectToSynthesizedMethod( nameof(this.Record) ) );

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(BillingDatabase) )
                .AllMethods()
                .RedirectToSynthesizedMethod( nameof(this.Record) ) );

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(SmtpClient) )
                .Methods( nameof(SmtpClient.Send) )
                .RedirectToSynthesizedMethod( nameof(this.Record) ) );
    }

    [Template]
    public dynamic? Record()
    {
        var interceptedMethod = meta.Interception.Method.InterceptedMethod;

        var arguments = new List<object?>();

        foreach ( var parameter in meta.Interception.Method.ArgumentParameters )
        {
            arguments.Add( parameter.Value );
        }

        var result = meta.Proceed();

        InteractionRecorder.Record(
            $"{interceptedMethod.DeclaringType.Name}.{interceptedMethod.Name}",
            arguments,
            result );

        return result;
    }
}
