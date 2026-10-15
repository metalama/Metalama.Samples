using System.Runtime.CompilerServices;

namespace Metalama.Samples.UniversalInterceptor;

// Calls the intercepted method with the receiver and the arguments read from args. The method writes its ref and out
// arguments to args.
public delegate TResult InterceptorDelegate<TArgs, out TResult>( ref TArgs args )
    where TArgs : struct, ITuple;
