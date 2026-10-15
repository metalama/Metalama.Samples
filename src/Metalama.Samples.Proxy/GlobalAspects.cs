using Metalama.Samples.Proxy;

[assembly: GenerateProxyAspect( typeof(ICalculator), "CalculatorProxy", "Proxies" )]
