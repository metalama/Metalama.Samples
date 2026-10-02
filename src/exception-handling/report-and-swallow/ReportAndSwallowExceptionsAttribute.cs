using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;

public class ReportAndSwallowExceptionsAttribute : OverrideMethodAspect
{
    [IntroduceDependency( IsRequired = false )]
    private readonly ILastChanceExceptionHandler? _exceptionHandler;

    public override dynamic? OverrideMethod()
    {
        try
        {
            return meta.Proceed();
        }
        catch ( Exception e ) when ( this._exceptionHandler != null &&
                                     this._exceptionHandler.ShouldHandle( e ) )
        {
            this._exceptionHandler.Report( e );

            return default;
        }
    }
}