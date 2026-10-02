using System.Runtime.CompilerServices;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Samples.Proxy;

[AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
public sealed class GenerateProxyAspect : CompilationAspect
{
    private readonly object _interfaceType;
    private readonly string _ns;
    private readonly string _typeName;

    public GenerateProxyAspect( Type interfaceType, string typeName, string? ns = null )
    {
        this._interfaceType = interfaceType;
        this._ns = ns ?? interfaceType.Namespace ?? "";
        this._typeName = typeName;
    }

    public GenerateProxyAspect( INamedType interfaceType, string typeName, string? ns = null )
    {
        this._interfaceType = interfaceType;
        this._ns = ns ?? interfaceType.ContainingNamespace.FullName;
        this._typeName = typeName;
    }

    public override void BuildAspect( IAspectBuilder<ICompilation> builder )
    {
        base.BuildAspect( builder );

        var interfaceType = this._interfaceType as INamedType ??
                            TypeFactory.GetNamedType( (Type) this._interfaceType );

        // Add a type.
        var type = builder.WithNamespace( this._ns )
            .IntroduceClass(
                this._typeName,
                buildType: t => t.Accessibility = Accessibility.Public );

        // Add a field with the intercepted object.
        var interceptedField = type.IntroduceField(
                "_intercepted",
                interfaceType,
                IntroductionScope.Instance )
            .Declaration;

        // Add a field for the interceptor.
        var interceptorField = type.IntroduceField( "_interceptor", typeof(IInterceptor) )
            .Declaration;

        // Implement the interface.
        type.ImplementInterface( interfaceType );

        // Implement interface members.
        var methodIndex = 0;

        var metadataList = new List<InterceptionMetadataInfo>();

        foreach ( var method in interfaceType.Methods )
        {
            methodIndex++;

            var argsType = TypeFactory.CreateTupleType( method.Parameters );
            var asyncInfo = method.GetAsyncInfo();
            var hasByRefParameter = method.Parameters.Any( p => p.RefKind != RefKind.None );

            string templateName;
            IType resultType;

            if ( !asyncInfo.IsAwaitable || hasByRefParameter )
            {
                if ( method.ReturnType.Equals( SpecialType.Void ) )
                {
                    templateName = nameof(this.VoidMethodTemplate);
                }
                else
                {
                    templateName = nameof(this.NonVoidMethodTemplate);
                }

                resultType = method.ReturnType;
            }
            else
            {
                var returnType = (INamedType) method.ReturnType;

                switch ( returnType.Definition.SpecialType )
                {
                    case SpecialType.Task:
                        templateName = nameof(this.TaskMethodTemplate);
                        resultType = TypeFactory.GetType( SpecialType.Void );

                        break;

                    case SpecialType.Task_T:
                        templateName = nameof(this.TaskOfTMethodTemplate);
                        resultType = returnType.TypeArguments[0];

                        break;

                    case SpecialType.ValueTask:
                        templateName = nameof(this.ValueTaskMethodTemplate);
                        resultType = TypeFactory.GetType( SpecialType.Void );

                        break;

                    case SpecialType.ValueTask_T:
                        templateName = nameof(this.ValueTaskOfTMethodTemplate);
                        resultType = returnType.TypeArguments[0];

                        break;

                    default:
                        builder.Diagnostics.Report(
                            DiagnosticDefinitions.AwaitableTypeNotSupported.WithArguments(
                                (
                                    interfaceType, method) ) );

                        continue;
                }
            }

            var metadataField =
                type.IntroduceField(
                        $"_metadata{methodIndex}",
                        typeof(InterceptionMetadata),
                        buildField: field => field.IsStatic = true )
                    .Declaration;

            metadataList.Add( new InterceptionMetadataInfo( method, metadataField, asyncInfo.IsAwaitable ) );

            type.IntroduceMethod(
                templateName,
                IntroductionScope.Instance,
                buildMethod: methodBuilder =>
                {
                    methodBuilder.Accessibility = Accessibility.Public;
                    methodBuilder.Name = method.Name;
                    methodBuilder.ReturnType = method.ReturnType;

                    foreach ( var parameter in method.Parameters )
                    {
                        methodBuilder.AddParameter(
                            parameter.Name,
                            parameter.Type,
                            parameter.RefKind );
                    }
                },
                args:
                new
                {
                    TArgs = argsType,
                    TResult = resultType,
                    method,
                    interceptedField,
                    interceptorField,
                    metadataField
                } );
        }

        // Add the constructor.
        type.IntroduceConstructor(
            nameof(this.Constructor),
            buildConstructor: constructorBuilder
                => constructorBuilder.AddParameter( "intercepted", interfaceType ),
            args: new { interceptedField, interceptorField } );

        type.AddInitializer(
            nameof(this.StaticConstructor),
            InitializerKind.BeforeTypeConstructor,
            args: new { metadataList } );
    }

    [Template]
    private void VoidMethodTemplate<[CompileTime] TArgs>(
        IMethod method,
        IField interceptedField,
        IField interceptorField,
        IField metadataField )
        where TArgs : struct, ITuple
    {
        // Prepare the context.
        var argsType = TypeFactory.CreateTupleType( method.Parameters );
        var args = (TArgs) CreateTupleInstance( method, argsType ).Value!;
        var argsExpression = ExpressionFactory.Capture( args );

        // Get writable parameters.
        var writableParameters = method.Parameters.Where( p => p.RefKind is RefKind.Out or RefKind.Ref )
            .ToList();

        // Invoke the interceptor.
        if ( writableParameters.Count == 0 )
        {
            // We don't need a try...finally if we don't have to write back writable parameters.
            ((IInterceptor) interceptorField.Value!).Invoke(
                ref args,
                (InterceptionMetadata) metadataField.Value!,
                Invoke );
        }
        else
        {
            try
            {
                ((IInterceptor) interceptorField.Value!).Invoke(
                    ref args,
                    (InterceptionMetadata) metadataField.Value!,
                    Invoke );
            }
            finally
            {
                // Copy back parameters.
                foreach ( var parameter in writableParameters )
                {
                    parameter.Value = argsType.TupleElements[parameter.Index].WithObject( argsExpression ).Value;
                }
            }
        }

        return;

        ValueTuple Invoke( ref TArgs receivedArgs )
        {
            var receivedArgsExpression = ExpressionFactory.Parse( "receivedArgs" );

            var arguments =
                method.Parameters.Select( p => argsType.TupleElements[p.Index].WithObject( receivedArgsExpression ) );

            method.WithObject( interceptedField ).Invoke( arguments );

            return default;
        }
    }

    private static IExpression CreateTupleInstance( IMethod method, ITupleType argsType )
    {
        return argsType.CreateCreateInstanceExpression(
            (IReadOnlyCollection<IExpression>) method.Parameters
                .Select( p => p.RefKind != RefKind.Out ? p : ExpressionFactory.Default( p.Type ) )
                .ToArray() );
    }

    [Template]
    private TResult NonVoidMethodTemplate<[CompileTime] TArgs, [CompileTime] TResult>(
        IMethod method,
        IField interceptedField,
        IField interceptorField,
        IField metadataField )
        where TArgs : struct, ITuple
    {
        // Prepare the context.
        var argsType = TypeFactory.CreateTupleType( method.Parameters );
        var args = (TArgs) CreateTupleInstance( method, argsType ).Value!;
        var argsExpression = ExpressionFactory.Capture( args );

        // Get writable parameters.
        var writableParameters = method.Parameters.Where( p =>
                                                              p.RefKind is RefKind.Out or RefKind.Ref )
            .ToList();

        // Invoke the interceptor.
        if ( writableParameters.Count == 0 )
        {
            // We don't need a try...finally if we don't have to write back writable parameters.
            return ((IInterceptor) interceptorField.Value!).Invoke(
                ref args,
                (InterceptionMetadata) metadataField.Value!,
                Invoke );
        }
        else
        {
            try
            {
                return ((IInterceptor) interceptorField.Value!).Invoke(
                    ref args,
                    (InterceptionMetadata) metadataField.Value!,
                    Invoke );
            }
            finally
            {
                // Copy back parameters.
                foreach ( var parameter in writableParameters )
                {
                    parameter.Value = argsType.TupleElements[parameter.Index].WithObject( argsExpression ).Value;
                }
            }
        }

        TResult Invoke( ref TArgs receivedArgs )
        {
            var receivedArgsExpression = ExpressionFactory.Parse( "receivedArgs" );

            var arguments =
                method.Parameters.Select( p => argsType.TupleElements[p.Index].WithObject( receivedArgsExpression ) );

            return method.WithObject( interceptedField ).Invoke( arguments )!;
        }
    }

    [Template]
    private async Task TaskMethodTemplate<[CompileTime] TArgs>(
        IMethod method,
        IField interceptedField,
        IField interceptorField,
        IField metadataField )
        where TArgs : struct, ITuple
    {
        // Prepare the context.
        var argsType = TypeFactory.CreateTupleType( method.Parameters );
        var args = (TArgs) CreateTupleInstance( method, argsType ).Value!;
        var argsExpression = ExpressionFactory.Capture( args );

        // Get writable parameters.
        var writableParameters = method.Parameters.Where( p =>
                                                              p.RefKind is RefKind.Out or RefKind.Ref )
            .ToList();

        // Invoke the interceptor.
        if ( writableParameters.Count == 0 )
        {
            // We don't need a try...finally if we don't have to write back writable parameters.
            await ((IInterceptor) interceptorField.Value!).InvokeAsync(
                args,
                (InterceptionMetadata) metadataField.Value!,
                InvokeAsync );
        }
        else
        {
            try
            {
                await ((IInterceptor) interceptorField.Value!).InvokeAsync(
                    args,
                    (InterceptionMetadata) metadataField.Value!,
                    InvokeAsync );
            }
            finally
            {
                // Copy back parameters.
                foreach ( var parameter in writableParameters )
                {
                    parameter.Value = argsType.TupleElements[parameter.Index].WithObject( argsExpression ).Value;
                }
            }
        }

        return;

        async Task<ValueTuple> InvokeAsync( TArgs receivedArgs )
        {
            var receivedArgsExpression = ExpressionFactory.Parse( "receivedArgs" );

            var arguments =
                method.Parameters.Select( p => argsType.TupleElements[p.Index].WithObject( receivedArgsExpression ) );

            await method.WithObject( interceptedField ).Invoke( arguments )!;

            return default;
        }
    }

    [Template]
    private async Task<TResult> TaskOfTMethodTemplate<[CompileTime] TArgs, [CompileTime] TResult>(
        IMethod method,
        IField interceptedField,
        IField interceptorField,
        IField metadataField )
        where TArgs : struct, ITuple
    {
        // Prepare the context.
        var argsType = TypeFactory.CreateTupleType( method.Parameters );
        var args = (TArgs) CreateTupleInstance( method, argsType ).Value!;
        var argsExpression = ExpressionFactory.Capture( args );

        // Get writable parameters.
        var writableParameters = method.Parameters.Where( p =>
                                                              p.RefKind is RefKind.Out or RefKind.Ref )
            .ToList();

        // Invoke the interceptor.
        if ( writableParameters.Count == 0 )
        {
            // We don't need a try...finally if we don't have to write back writable parameters.
            return await ((IInterceptor) interceptorField.Value!).InvokeAsync(
                args,
                (InterceptionMetadata) metadataField.Value!,
                InvokeAsync );
        }
        else
        {
            try
            {
                return await ((IInterceptor) interceptorField.Value!).InvokeAsync(
                    args,
                    (InterceptionMetadata) metadataField.Value!,
                    InvokeAsync );
            }
            finally
            {
                // Copy back parameters.
                foreach ( var parameter in writableParameters )
                {
                    parameter.Value = argsType.TupleElements[parameter.Index].WithObject( argsExpression ).Value;
                }
            }
        }

        Task<TResult> InvokeAsync( TArgs receivedArgs )
        {
            var receivedArgsExpression = ExpressionFactory.Parse( "receivedArgs" );

            var arguments =
                method.Parameters.Select( p => argsType.TupleElements[p.Index].WithObject( receivedArgsExpression ) );

            return method.WithObject( interceptedField ).Invoke( arguments )!;
        }
    }

    [Template]
    private async ValueTask ValueTaskMethodTemplate<[CompileTime] TArgs>(
        IMethod method,
        IField interceptedField,
        IField interceptorField,
        IField metadataField )
        where TArgs : struct, ITuple
    {
        // Prepare the context.
        var argsType = TypeFactory.CreateTupleType( method.Parameters );
        var args = (TArgs) CreateTupleInstance( method, argsType ).Value!;
        var argsExpression = ExpressionFactory.Capture( args );

        // Get writable parameters.
        var writableParameters = method.Parameters.Where( p =>
                                                              p.RefKind is RefKind.Out or RefKind.Ref )
            .ToList();

        // Invoke the interceptor.
        if ( writableParameters.Count == 0 )
        {
            // We don't need a try...finally if we don't have to write back writable parameters.
            await ((IInterceptor) interceptorField.Value!).InvokeAsync(
                args,
                (InterceptionMetadata) metadataField.Value!,
                InvokeAsync );
        }
        else
        {
            try
            {
                await ((IInterceptor) interceptorField.Value!).InvokeAsync(
                    args,
                    (InterceptionMetadata) metadataField.Value!,
                    InvokeAsync );
            }
            finally
            {
                // Copy back parameters.
                foreach ( var parameter in writableParameters )
                {
                    parameter.Value = argsType.TupleElements[parameter.Index].WithObject( argsExpression ).Value;
                }
            }
        }

        return;

        async ValueTask<ValueTuple> InvokeAsync( TArgs receivedArgs )
        {
            var receivedArgsExpression = ExpressionFactory.Parse( "receivedArgs" );

            var arguments =
                method.Parameters.Select( p => argsType.TupleElements[p.Index].WithObject( receivedArgsExpression ) );

            await method.WithObject( interceptedField ).Invoke( arguments )!;

            return default;
        }
    }

    [Template]
    private async ValueTask<TResult> ValueTaskOfTMethodTemplate<[CompileTime] TArgs,
                                                                [CompileTime] TResult>(
        IMethod method,
        IField interceptedField,
        IField interceptorField,
        IField metadataField )
        where TArgs : struct, ITuple
    {
        // Prepare the context.
        var argsType = TypeFactory.CreateTupleType( method.Parameters );
        var args = (TArgs) CreateTupleInstance( method, argsType ).Value!;
        var argsExpression = ExpressionFactory.Capture( args );

        // Get writable parameters.
        var writableParameters = method.Parameters.Where( p =>
                                                              p.RefKind is RefKind.Out or RefKind.Ref )
            .ToList();

        // Invoke the interceptor.
        if ( writableParameters.Count == 0 )
        {
            // We don't need a try...finally if we don't have to write back writable parameters.
            return await ((IInterceptor) interceptorField.Value!).InvokeAsync(
                args,
                (InterceptionMetadata) metadataField.Value!,
                InvokeAsync );
        }
        else
        {
            try
            {
                return await ((IInterceptor) interceptorField.Value!).InvokeAsync(
                    args,
                    (InterceptionMetadata) metadataField.Value!,
                    InvokeAsync );
            }
            finally
            {
                // Copy back parameters.
                foreach ( var parameter in writableParameters )
                {
                    parameter.Value = argsType.TupleElements[parameter.Index].WithObject( argsExpression ).Value;
                }
            }
        }

        ValueTask<TResult> InvokeAsync( TArgs receivedArgs )
        {
            var receivedArgsExpression = ExpressionFactory.Parse( "receivedArgs" );

            var arguments =
                method.Parameters.Select( p => argsType.TupleElements[p.Index].WithObject( receivedArgsExpression ) );

            return method.WithObject( interceptedField ).Invoke( arguments )!;
        }
    }

    [Template]
    public void Constructor(
        IInterceptor interceptor,
        IField interceptedField,
        IField interceptorField )
    {
        interceptorField.Value = interceptor;
        interceptedField.Value = meta.Target.Parameters["intercepted"].Value;
    }

    [Template]
    private void StaticConstructor( List<InterceptionMetadataInfo> metadataList )
    {
        foreach ( var item in metadataList )
        {
            item.MetadataField.Value =
                new InterceptionMetadata( item.Method.ToMethodInfo(), item.ReturnsAwaitable );
        }
    }
}