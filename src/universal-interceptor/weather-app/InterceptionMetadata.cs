namespace Metalama.Samples.UniversalInterceptor;

// Describes the intercepted method. It is a struct, so the generated code creates it at each call without allocating
// memory on the heap.
public readonly struct InterceptionMetadata
{
    public InterceptionMetadata( Type declaringType, string methodName, string signature, bool hasReceiver )
    {
        this.DeclaringType = declaringType;
        this.MethodName = methodName;
        this.Signature = signature;
        this.HasReceiver = hasReceiver;
    }

    // The type that declares the intercepted method.
    public Type DeclaringType { get; }

    // The name of the intercepted method.
    public string MethodName { get; }

    // The name of the intercepted method with the types of its parameters, for instance
    // "WeatherClient.GetTemperature(string)".
    public string Signature { get; }

    // Indicates whether the first element of the argument tuple is the receiver of the call, which is the object on
    // which the method is called.
    public bool HasReceiver { get; }

    public override string ToString() => this.Signature;
}
