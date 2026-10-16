using Metalama.Extensions.Interceptors;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Code.SyntaxBuilders;
using System.Runtime.CompilerServices;

namespace Metalama.Samples.UniversalInterceptor;

// The T# templates of the generated interceptors. Each interceptor packs the receiver and the arguments of the call into
// a value tuple of type TArgs. It passes the tuple to the IUniversalInterceptor instance, with a local function that
// reads the tuple and calls the intercepted method. The RedirectToUniversalInterceptor method chooses the template from
// the return type of the intercepted method.
internal sealed class UniversalInterceptorTemplates : ITemplateProvider
{
    // For a synchronous method that returns a value.
    [Template]
    public static TResult Invoke<[CompileTime] TArgs, [CompileTime] TResult>(
        [CompileTime] Type interceptorType,
        [CompileTime] string memberName,
        [CompileTime] bool isAsyncLocal )
        where TArgs : struct, ITuple
    {
        var argsType = TypeFactory.CreateTupleType( meta.Target.Method.Parameters );
        var args = (TArgs) CreateArgs( argsType ).Value!;
        var metadata = (InterceptionMetadata) CreateMetadata().Value!;
        IUniversalInterceptor interceptor = GetInterceptor( interceptorType, memberName, isAsyncLocal ).Value!;

        if ( !HasWritableParameters() )
        {
            return interceptor.Invoke( ref args, metadata, Proceed );
        }

        var argsExpression = ExpressionFactory.Capture( args );

        try
        {
            return interceptor.Invoke( ref args, metadata, Proceed );
        }
        finally
        {
            WriteBackArguments( argsType, argsExpression );
        }

        static TResult Proceed( ref TArgs receivedArgs )
        {
            return GetInvoker( "receivedArgs", out var arguments )
                .Invoke( arguments )!;
        }
    }

    // For a synchronous method that returns void. The result of the interceptor is a ValueTuple, which is ignored.
    [Template]
    public static void InvokeVoid<[CompileTime] TArgs>(
        [CompileTime] Type interceptorType,
        [CompileTime] string memberName,
        [CompileTime] bool isAsyncLocal )
        where TArgs : struct, ITuple
    {
        var argsType = TypeFactory.CreateTupleType( meta.Target.Method.Parameters );
        var args = (TArgs) CreateArgs( argsType ).Value!;
        var metadata = (InterceptionMetadata) CreateMetadata().Value!;
        IUniversalInterceptor interceptor = GetInterceptor( interceptorType, memberName, isAsyncLocal ).Value!;

        if ( !HasWritableParameters() )
        {
            interceptor.Invoke( ref args, metadata, Proceed );
        }
        else
        {
            var argsExpression = ExpressionFactory.Capture( args );

            try
            {
                interceptor.Invoke( ref args, metadata, Proceed );
            }
            finally
            {
                WriteBackArguments( argsType, argsExpression );
            }
        }

        static ValueTuple Proceed( ref TArgs receivedArgs )
        {
            GetInvoker( "receivedArgs", out var arguments ).Invoke( arguments );

            return default;
        }
    }

    // For a method that returns a Task<T>.
    [Template]
    public static async Task<TResult> InvokeTaskOfTAsync<[CompileTime] TArgs, [CompileTime] TResult>(
        [CompileTime] Type interceptorType,
        [CompileTime] string memberName,
        [CompileTime] bool isAsyncLocal )
        where TArgs : struct, ITuple
    {
        var argsType = TypeFactory.CreateTupleType( meta.Target.Method.Parameters );
        var args = (TArgs) CreateArgs( argsType ).Value!;
        var metadata = (InterceptionMetadata) CreateMetadata().Value!;
        IUniversalInterceptor interceptor = GetInterceptor( interceptorType, memberName, isAsyncLocal ).Value!;

        return await interceptor.InvokeAsync( args, metadata, ProceedAsync );

        static Task<TResult> ProceedAsync( TArgs receivedArgs )
        {
            return GetInvoker( "receivedArgs", out var arguments )
                .Invoke( arguments )!;
        }
    }

    // For a method that returns a Task. The result of the interceptor is a ValueTuple, which is ignored.
    [Template]
    public static async Task InvokeTaskAsync<[CompileTime] TArgs>(
        [CompileTime] Type interceptorType,
        [CompileTime] string memberName,
        [CompileTime] bool isAsyncLocal )
        where TArgs : struct, ITuple
    {
        var argsType = TypeFactory.CreateTupleType( meta.Target.Method.Parameters );
        var args = (TArgs) CreateArgs( argsType ).Value!;
        var metadata = (InterceptionMetadata) CreateMetadata().Value!;
        IUniversalInterceptor interceptor = GetInterceptor( interceptorType, memberName, isAsyncLocal ).Value!;

        await interceptor.InvokeAsync( args, metadata, ProceedAsync );

        static async Task<ValueTuple> ProceedAsync( TArgs receivedArgs )
        {
            await GetInvoker( "receivedArgs", out var arguments )
                .Invoke( arguments )!;

            return default;
        }
    }

    // For a method that returns a ValueTask<T>.
    [Template]
    public static async ValueTask<TResult> InvokeValueTaskOfTAsync<[CompileTime] TArgs, [CompileTime] TResult>(
        [CompileTime] Type interceptorType,
        [CompileTime] string memberName,
        [CompileTime] bool isAsyncLocal )
        where TArgs : struct, ITuple
    {
        var argsType = TypeFactory.CreateTupleType( meta.Target.Method.Parameters );
        var args = (TArgs) CreateArgs( argsType ).Value!;
        var metadata = (InterceptionMetadata) CreateMetadata().Value!;
        IUniversalInterceptor interceptor = GetInterceptor( interceptorType, memberName, isAsyncLocal ).Value!;

        return await interceptor.InvokeAsync( args, metadata, ProceedAsync );

        static ValueTask<TResult> ProceedAsync( TArgs receivedArgs )
        {
            return GetInvoker( "receivedArgs", out var arguments )
                .Invoke( arguments )!;
        }
    }

    // For a method that returns a ValueTask. The result of the interceptor is a ValueTuple, which is ignored.
    [Template]
    public static async ValueTask InvokeValueTaskAsync<[CompileTime] TArgs>(
        [CompileTime] Type interceptorType,
        [CompileTime] string memberName,
        [CompileTime] bool isAsyncLocal )
        where TArgs : struct, ITuple
    {
        var argsType = TypeFactory.CreateTupleType( meta.Target.Method.Parameters );
        var args = (TArgs) CreateArgs( argsType ).Value!;
        var metadata = (InterceptionMetadata) CreateMetadata().Value!;
        IUniversalInterceptor interceptor = GetInterceptor( interceptorType, memberName, isAsyncLocal ).Value!;

        await interceptor.InvokeAsync( args, metadata, ProceedAsync );

        static async ValueTask<ValueTuple> ProceedAsync( TArgs receivedArgs )
        {
            await GetInvoker( "receivedArgs", out var arguments )
                .Invoke( arguments )!;

            return default;
        }
    }

    // Creates the expression of the argument tuple from the parameters of the generated interceptor. An out parameter
    // has no value before the call, so its element is the default value.
    [CompileTime]
    private static IExpression CreateArgs( ITupleType argsType )
        => argsType.CreateCreateInstanceExpression(
            meta.Target.Method.Parameters.Select(
                p => p.RefKind == RefKind.Out ? ExpressionFactory.Default( p.Type ) : (IExpression) p ) );

    // Creates the expression that creates the InterceptionMetadata of the intercepted method.
    [CompileTime]
    private static IExpression CreateMetadata()
    {
        var method = meta.Interception.Method.InterceptedMethod;

        var builder = new ExpressionBuilder();
        builder.AppendVerbatim( "new " );
        builder.AppendTypeName( typeof(InterceptionMetadata) );
        builder.AppendVerbatim( "(" );
        builder.AppendExpression( method.DeclaringType.ToTypeOfExpression() );
        builder.AppendVerbatim( ", " );
        builder.AppendLiteral( method.Name );
        builder.AppendVerbatim( ", " );
        builder.AppendLiteral( method.ToDisplayString() );
        builder.AppendVerbatim( ", " );
        builder.AppendVerbatim( meta.Interception.Method.ReceiverParameter != null ? "true" : "false" );
        builder.AppendVerbatim( ")" );

        return builder.ToExpression();
    }

    // Returns the expression that gives the interceptor: the static member, or its Value property when the member is an
    // AsyncLocal<IUniversalInterceptor>.
    [CompileTime]
    private static IExpression GetInterceptor( Type interceptorType, string memberName, bool isAsyncLocal )
    {
        var member = TypeFactory.GetNamedType( interceptorType ).FieldsAndProperties.OfName( memberName ).First();

        if ( !isAsyncLocal )
        {
            return member;
        }

        var builder = new ExpressionBuilder();
        builder.AppendExpression( member );
        builder.AppendVerbatim( ".Value" );

        return builder.ToExpression();
    }

    [CompileTime]
    private static bool HasWritableParameters()
        => meta.Target.Method.Parameters.Any( p => p.RefKind is RefKind.Ref or RefKind.Out );

    // Assigns the ref and out parameters of the generated interceptor from the tuple, after the call.
    [Template]
    private static void WriteBackArguments( [CompileTime] ITupleType argsType, [CompileTime] IExpression args )
    {
        foreach ( var parameter in meta.Target.Method.Parameters.Where( p => p.RefKind is RefKind.Ref or RefKind.Out ) )
        {
            parameter.Value = argsType.TupleElements[parameter.Index].WithObject( args ).Value;
        }
    }

    // Returns the invoker of the intercepted method, and the arguments read from the tuple, which is the parameter of
    // the local function named tupleParameterName. For an instance call, the invoker calls the method on the receiver
    // read from the tuple.
    [CompileTime]
    private static IMethodInvoker GetInvoker( string tupleParameterName, out IEnumerable<IExpression> arguments )
    {
        var interception = meta.Interception.Method;
        var argsType = TypeFactory.CreateTupleType( meta.Target.Method.Parameters );

        // The expression must have the tuple type and be referenceable, so that its elements can be passed by ref or out.
        var tuple = ExpressionFactory.Parse( tupleParameterName, argsType, isReferenceable: true )!;

        var receiver = interception.ReceiverParameter == null
            ? null
            : argsType.TupleElements[interception.ReceiverParameter.Index].WithObject( tuple );

        var argumentList = interception.ArgumentParameters
            .Select( p => argsType.TupleElements[p.Index].WithObject( tuple ) )
            .ToList();

        // The intercepted method of a call to an extension method in reduced form is the static method. The receiver
        // is its first argument.
        if ( receiver != null && interception.InterceptedMethod.Parameters is [{ IsThis: true }, ..] )
        {
            argumentList.Insert( 0, receiver );
            arguments = argumentList;

            return interception.InterceptedMethod;
        }

        arguments = argumentList;

        return receiver == null ? interception.InterceptedMethod : interception.InterceptedMethod.WithObject( receiver );
    }
}
