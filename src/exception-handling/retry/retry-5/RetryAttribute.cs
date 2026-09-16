using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Microsoft.Extensions.Logging;

public class RetryAttribute : OverrideMethodAspect
{
    [IntroduceDependency]
    private readonly ILogger _logger;

    [IntroduceDependency]
    private readonly IResiliencePipelineFactory _resiliencePipelineFactory;

    public StrategyKind Kind { get; }

    public RetryAttribute( StrategyKind kind = StrategyKind.Retry )
    {
        this.Kind = kind;
    }

    // Template for non-async methods.
    public override dynamic? OverrideMethod()
    {
        if ( meta.Target.Method.ReturnType.SpecialType == SpecialType.Void )
        {
            void ExecuteVoid()
            {
                try
                {
                    meta.Proceed();
                }
                catch ( Exception e )
                {
                    var messageBuilder = LoggingHelper.BuildInterpolatedString( false );
                    messageBuilder.AddText( " has failed: " );
                    messageBuilder.AddExpression( e.Message );
                    this._logger.LogWarning( (string) messageBuilder.ToValue() );

                    throw;
                }
            }

            var pipeline = this._resiliencePipelineFactory.GetPipeline( this.Kind );
            pipeline.Execute( ExecuteVoid );

            return null; // Dummy
        }
        else
        {
            object? ExecuteCore()
            {
                try
                {
                    return meta.Proceed();
                }
                catch ( Exception e )
                {
                    var messageBuilder = LoggingHelper.BuildInterpolatedString( false );
                    messageBuilder.AddText( " has failed: " );
                    messageBuilder.AddExpression( e.Message );
                    this._logger.LogWarning( (string) messageBuilder.ToValue() );

                    throw;
                }
            }

            var pipeline = this._resiliencePipelineFactory.GetPipeline( this.Kind );

            return pipeline.Execute( ExecuteCore );
        }
    }

    // Template for async methods.
    public override async Task<dynamic?> OverrideAsyncMethod()
    {
        async Task<object?> ExecuteCoreAsync( CancellationToken cancellationToken )
        {
            try
            {
                return await meta.ProceedAsync();
            }
            catch ( Exception e )
            {
                var messageBuilder = LoggingHelper.BuildInterpolatedString( false );
                messageBuilder.AddText( " has failed: " );
                messageBuilder.AddExpression( e.Message );
                this._logger.LogWarning( (string) messageBuilder.ToValue() );

                throw;
            }
        }

        var cancellationTokenParameter
            = meta.Target.Parameters.LastOrDefault( p => p.Type.Equals( typeof(CancellationToken) ) );

        var pipeline = this._resiliencePipelineFactory.GetPipeline( this.Kind );

        return await pipeline.ExecuteAsync(
            async token => await ExecuteCoreAsync( token ),
            cancellationTokenParameter != null
                ? (CancellationToken) cancellationTokenParameter.Value!
                : CancellationToken.None );
    }
}