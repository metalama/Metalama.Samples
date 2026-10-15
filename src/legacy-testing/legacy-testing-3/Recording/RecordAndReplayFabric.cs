using Metalama.Extensions.Interceptors;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using System.Net.Mail;

namespace Metalama.Samples.LegacyTesting3.Recording;

// Records the calls to the hard-wired dependencies of the legacy code in the Record configuration,
// and replays them in the Test configuration.
internal class RecordAndReplayFabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        string template;

        if ( amender.Project.PreprocessorSymbols.Contains( "RECORD_INTERACTIONS" ) )
        {
            template = nameof(this.Record);
        }
        else if ( amender.Project.PreprocessorSymbols.Contains( "REPLAY_INTERACTIONS" ) )
        {
            template = nameof(this.Replay);
        }
        else
        {
            return;
        }

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(Thread) )
                .Methods( nameof(Thread.Sleep) )
                .RedirectToSynthesizedMethod( template ) );

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(File) )
                .Methods( nameof(File.ReadAllText) )
                .RedirectToSynthesizedMethod( template ) );

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(BillingDatabase) )
                .AllMethods()
                .RedirectToSynthesizedMethod( template ) );

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(SmtpClient) )
                .Methods( nameof(SmtpClient.Send) )
                .RedirectToSynthesizedMethod( template ) );
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

    [Template]
    public dynamic? Replay()
    {
        var interceptedMethod = meta.Interception.Method.InterceptedMethod;

        var arguments = new List<object?>();

        foreach ( var parameter in meta.Interception.Method.ArgumentParameters )
        {
            arguments.Add( parameter.Value );
        }

        // The intercepted method is not called.
        if ( interceptedMethod.ReturnType.Equals( SpecialType.Void ) )
        {
            InteractionReplayer.Replay(
                $"{interceptedMethod.DeclaringType.Name}.{interceptedMethod.Name}",
                arguments );

            return null;
        }
        else
        {
            var result = InteractionReplayer.Replay(
                $"{interceptedMethod.DeclaringType.Name}.{interceptedMethod.Name}",
                arguments,
                interceptedMethod.ReturnType.ToType() );

            return meta.Cast( interceptedMethod.ReturnType, result );
        }
    }
}
