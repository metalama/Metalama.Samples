using System.Runtime.CompilerServices;

namespace Metalama.Samples.Proxy;

public delegate TResult InterceptorDelegate<TArgs, out TResult>( ref TArgs args )
    where TArgs : struct, ITuple;