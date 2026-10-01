public interface ILastChanceExceptionHandler
{
    /// <summary>
    /// Determines if an exception should be handled. Typically returns <c>false</c> 
    /// for exception like <see cref="OperationCanceledException"/>.
    /// </summary>
    bool ShouldHandle( Exception e );

    /// <summary>
    /// Reports the exception to the user or to the crash report service.
    /// </summary>
    void Report( Exception e );
}