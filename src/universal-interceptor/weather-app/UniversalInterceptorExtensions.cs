using Metalama.Extensions.Interceptors;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Samples.UniversalInterceptor;

[CompileTime]
public static class UniversalInterceptorExtensions
{
    // Redirects the selected calls to the IUniversalInterceptor instance that a static field or property gives.
    // The interceptorType type must implement IUniversalInterceptor. The memberName member is a static field or
    // property of this type, of type IUniversalInterceptor or AsyncLocal<IUniversalInterceptor>.
    public static IMethodInterception RedirectToUniversalInterceptor(
        this IMethodResultFactory factory,
        Type interceptorType,
        string memberName )
        => factory switch
        {
            // On a call site, in the delegate given to ForEachSite.
            IMethodInterceptionSite site => RedirectSite( site, interceptorType, memberName ),

            // On the method selection. The template and its arguments depend on the intercepted method, so the
            // interception is decided for each call site.
            IMethodSelection selection => selection.ForEachSite(
                site => RedirectSite( site, interceptorType, memberName ) ),

            _ => throw new ArgumentOutOfRangeException( nameof(factory) )
        };

    private static IMethodInterception RedirectSite(
        IMethodInterceptionSite site,
        Type interceptorType,
        string memberName )
    {
        if ( !TryGetInterceptorMember( site, interceptorType, memberName, out var isAsyncLocal ) )
        {
            return site.Skip();
        }

        var method = site.InterceptedMethod;

        if ( site.UseKind != MethodUseKind.Call )
        {
            // Interceptors generated from templates support only calls.
            return Skip( site, "it is a method group, not a call" );
        }

        if ( !method.IsStatic && site.Receiver.Type is { IsReferenceType: false } )
        {
            // The tuple would contain a copy of the receiver, so the changes that the method makes to the receiver
            // would be lost. The receiver of an extension method is an ordinary argument, so this rule does not
            // apply to it.
            return Skip( site, "its receiver is a struct, and the argument tuple would contain a copy of the receiver" );
        }

        string template;
        IType? resultType;

        if ( method.GetAsyncInfo().IsAwaitable )
        {
            if ( !TryChooseAsyncTemplate( method, out template, out resultType, out var reason ) )
            {
                return Skip( site, reason );
            }
        }
        else
        {
            ChooseSynchronousTemplate( method, out template, out resultType );
        }

        return Redirect( site, template, resultType, interceptorType, memberName, isAsyncLocal );
    }

    // Chooses the template of a synchronous method. A void method gets the InvokeVoid template, and has no result type.
    private static void ChooseSynchronousTemplate( IMethod method, out string template, out IType? resultType )
    {
        if ( method.ReturnType.Equals( SpecialType.Void ) )
        {
            template = nameof(UniversalInterceptorTemplates.InvokeVoid);
            resultType = null;
        }
        else
        {
            template = nameof(UniversalInterceptorTemplates.Invoke);
            resultType = method.ReturnType;
        }
    }

    // Chooses the template of a method that returns an awaitable type. The result type is the type of the awaited
    // value, or null for a Task or a ValueTask. Returns false, with the reason, when the method is not supported.
    private static bool TryChooseAsyncTemplate(
        IMethod method,
        out string template,
        out IType? resultType,
        out string reason )
    {
        template = "";
        resultType = null;
        reason = "";

        if ( method.Parameters.Any( p => p.RefKind is RefKind.Ref or RefKind.Out ) )
        {
            // The delegate that calls an async method receives the tuple by value, so the ref and out arguments
            // cannot be written back.
            reason = "the method returns an awaitable type and has ref or out parameters";

            return false;
        }

        var asyncInfo = method.GetAsyncInfo();

        switch ( (method.ReturnType as INamedType)?.Definition.SpecialType )
        {
            case SpecialType.Task_T:
                template = nameof(UniversalInterceptorTemplates.InvokeTaskOfTAsync);
                resultType = asyncInfo.ResultType;

                return true;

            case SpecialType.Task:
                template = nameof(UniversalInterceptorTemplates.InvokeTaskAsync);

                return true;

            case SpecialType.ValueTask_T:
                template = nameof(UniversalInterceptorTemplates.InvokeValueTaskOfTAsync);
                resultType = asyncInfo.ResultType;

                return true;

            case SpecialType.ValueTask:
                template = nameof(UniversalInterceptorTemplates.InvokeValueTaskAsync);

                return true;

            default:
                reason = "its return type is awaitable but is not a Task or a ValueTask";

                return false;
        }
    }

    // Redirects the call to a method generated from the template. The template receives the type of the argument tuple,
    // the result type, if any, and the member that gives the interceptor.
    private static IMethodInterception Redirect(
        IMethodInterceptionSite site,
        string template,
        IType? resultType,
        Type interceptorType,
        string memberName,
        bool isAsyncLocal )
    {
        var argsType = CreateArgsType( site );

        // LAMA0872 is reported because the analyzer cannot verify that a TemplateProvider or an anonymous type is
        // durable. This result is created in the delegate of ForEachSite, for one call site of the current compilation.
#pragma warning disable LAMA0872
        var interception = site
            .RedirectToSynthesizedMethod( template )
            .WithTemplateProvider( TemplateProvider.FromType<UniversalInterceptorTemplates>() );

        return resultType == null
            ? interception.WithArgs( new { TArgs = argsType, interceptorType, memberName, isAsyncLocal } )
            : interception.WithArgs(
                new { TArgs = argsType, TResult = resultType, interceptorType, memberName, isAsyncLocal } );
#pragma warning restore LAMA0872
    }

    // Checks that interceptorType implements IUniversalInterceptor and has a static member named memberName, of type
    // IUniversalInterceptor or AsyncLocal<IUniversalInterceptor>. Reports an error otherwise.
    private static bool TryGetInterceptorMember(
        IMethodInterceptionSite site,
        Type interceptorType,
        string memberName,
        out bool isAsyncLocal )
    {
        isAsyncLocal = false;

        var type = TypeFactory.GetNamedType( interceptorType );
        var interfaceType = TypeFactory.GetNamedType( typeof(IUniversalInterceptor) );

        if ( !type.IsConvertibleTo( interfaceType ) )
        {
            site.Diagnostics.Report( DiagnosticDefinitions.InterceptorTypeNotImplemented.WithArguments( type ) );

            return false;
        }

        var member = type.FieldsAndProperties.OfName( memberName ).FirstOrDefault();

        if ( member == null )
        {
            site.Diagnostics.Report(
                DiagnosticDefinitions.InterceptorMemberNotFound.WithArguments( (type, memberName) ) );

            return false;
        }

        if ( !member.IsStatic )
        {
            site.Diagnostics.Report( DiagnosticDefinitions.InterceptorMemberNotStatic.WithArguments( member ) );

            return false;
        }

        var memberType = member.Type.ToNonNullable();

        if ( memberType.Equals( interfaceType ) )
        {
            return true;
        }

        if ( memberType is INamedType { TypeArguments.Count: 1 } namedType
             && namedType.Definition.Equals( TypeFactory.GetNamedType( typeof(AsyncLocal<>) ) )
             && namedType.TypeArguments[0].ToNonNullable().Equals( interfaceType ) )
        {
            isAsyncLocal = true;

            return true;
        }

        site.Diagnostics.Report(
            DiagnosticDefinitions.InterceptorMemberTypeInvalid.WithArguments( (member, member.Type) ) );

        return false;
    }

    // Creates the type of the argument tuple: the receiver, if any, then the arguments. The elements have the names of
    // the parameters of the generated interceptor, so the templates create the same type from these parameters.
    private static ITupleType CreateArgsType( IMethodInterceptionSite site )
    {
        var method = site.InterceptedMethod;

        // For a call to an extension method in reduced form, such as text.Wrap( ">" ), the intercepted method is the
        // static method, and its first parameter receives the receiver of the call.
        var isReducedExtension = method.Parameters.Count > 0
                                 && method.Parameters[0].IsThis
                                 && site.Receiver.Kind != InvocationReceiverKind.None;

        var elements = new List<(IType Type, string Name)>();

        if ( !method.IsStatic || isReducedExtension )
        {
            // The generated interceptor names its receiver parameter "receiver". When a parameter of the intercepted
            // method already has this name, the name gets leading underscores.
            var receiverName = "receiver";

            while ( method.Parameters.Any( p => p.Name == receiverName ) )
            {
                receiverName = "_" + receiverName;
            }

            elements.Add( (isReducedExtension ? method.Parameters[0].Type : method.DeclaringType, receiverName) );
        }

        elements.AddRange( method.Parameters.Skip( isReducedExtension ? 1 : 0 ).Select( p => (p.Type, p.Name) ) );

        return TypeFactory.CreateTupleType( elements );
    }

    private static IMethodInterception Skip( IMethodInterceptionSite site, string reason )
    {
        site.Diagnostics.Report(
            DiagnosticDefinitions.CallNotIntercepted.WithArguments( (site.InterceptedMethod, reason) ) );

        return site.Skip();
    }
}
