using Microsoft.Extensions.Logging;

internal class Calculator
{
    private readonly ILogger<Calculator> _logger;

    public Calculator( ILogger<Calculator> logger )
    {
        this._logger = logger;
    }

    [Log]
    public double Add( double a, double b ) => a + b;
}