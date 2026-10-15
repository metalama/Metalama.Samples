---
uid: sample-proxy
browse-source: .
level: 400
summary: "This example builds an aspect that generates an interception proxy for an interface at compile time, routing every call through an IInterceptor without dynamic code generation."
keywords: "proxy pattern C#, interception proxy, DispatchProxy alternative, compile-time proxy generation, interceptor, aspect"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Generating interception proxies without reflection

[!metalama-project-buttons .]

An interception proxy is a class that implements an interface, forwards each call to another object that implements the same interface, and lets a third party observe or alter the call. It is the mechanism behind logging decorators, caching layers, remoting stubs and test doubles.

The .NET platform offers <xref:System.Reflection.DispatchProxy>, which builds the proxy class at run time. That approach requires run-time code generation, which is not available in a trimmed or ahead-of-time compiled application, and it packs every argument into an `object[]`, which allocates an array and boxes value types.

This example generates the proxy class at compile time instead. The aspect reads the interface from the code model and introduces a new class that implements it. Each generated method packs its arguments into a `ValueTuple`, which is a value type, so no boxing occurs for value-type arguments.

## The interception contracts

The generated proxy calls an interceptor through the following interface. The arguments are passed through a generic `TArgs` type parameter constrained to `struct, ITuple`, so the interceptor receives the argument tuple without boxing it.

[!metalama-file IInterceptor.cs]

The synchronous overload takes the argument tuple by reference, which is what allows an interceptor to modify `out` and `ref` arguments. The callback that invokes the intercepted object therefore also takes its argument by reference, so it needs a dedicated delegate type.

[!metalama-file InterceptorDelegate.cs]

Each intercepted method also passes metadata that identifies it. The proxy stores one metadata instance per method in a static field, so the reflection lookup happens once per type instead of once per call.

[!metalama-file InterceptionMetadata.cs]

## Applying the aspect

This example proxies the following interface.

[!metalama-file ICalculator.cs]

The aspect is an assembly-level attribute. It takes the interface to proxy, the name of the class to generate, and optionally the namespace to generate it in.

[!metalama-file GlobalAspects.cs]

From that single attribute, the aspect generates the following class.

[!metalama-file Proxies.CalculatorProxy.cs transformed="true"]

Three details of the generated code are worth noting. The argument tuple is a local variable, so it lives on the stack. The callback that reaches the intercepted object is a local function, so it captures that local variable instead of allocating a closure object on every call. A method that takes no parameter gets an empty tuple, built by `ValueTuple.Create()`.

## Writing an interceptor

The aspect generates the plumbing, but the behaviour comes from the interceptor. The following one writes a line before and after each call.

[!metalama-file LoggingInterceptor.cs]

The three methods receive the arguments as a tuple constrained to `ITuple`, which exposes `Length` and an indexer, so an interceptor can read the arguments without knowing the signature of the method it intercepts. The `InterceptionMetadata` parameter supplies the <xref:System.Reflection.MethodInfo> of the intercepted method.

A void method is projected onto a `ValueTuple` result, because the interceptor is written in terms of a return value. That is why `FormatResult` treats `ValueTuple` as a special case.

The generated proxy is then used like any hand-written class.

```csharp
var calculator = new CalculatorProxy( new LoggingInterceptor( Console.Out ), new Calculator() );
var sum = calculator.Add( 2, 3 );
```

The project `Metalama.Samples.Proxy.UnitTests` executes the generated proxy and asserts this behaviour: that the return value flows back through the interceptor, that the arguments reach the intercepted object, that the interceptor observes the method name and the argument values, and that an exception thrown by the intercepted object is reported and then propagates.

## Step 1. Introducing the proxy class

The aspect derives from <xref:Metalama.Framework.Aspects.CompilationAspect> because the class it generates does not belong to any existing type. Its `BuildAspect` method resolves the interface, introduces the class and its fields, then introduces one method per interface method.

[!metalama-file GenerateProxyAspect.cs member="GenerateProxyAspect.BuildAspect"]

The interface is stored in an `object` field because the aspect has two constructors. User code supplies a <xref:System.Type>, while a fabric supplies an <xref:Metalama.Framework.Code.INamedType>. `BuildAspect` accepts either and converts a `Type` with `TypeFactory.GetNamedType`.

`builder.WithNamespace( ... ).IntroduceClass( ... )` introduces the class in the requested namespace, as explained in <xref:introducing-types>, and `type.ImplementInterface( interfaceType )` declares that the class implements the interface, as explained in <xref:implementing-interfaces>.

For each method, `TypeFactory.CreateTupleType( method.Parameters )` builds the tuple type that carries the arguments. The tuple element names and types come from the parameters, which is why the generated code reads `receivedArgs.a` instead of `receivedArgs.Item1`.

The template is then selected from the return type. There are six templates: two synchronous ones for `void` and non-`void` returns, and four asynchronous ones for `Task`, `Task<TResult>`, `ValueTask` and `ValueTask<TResult>`. The tuple type and the result type are passed to the template as compile-time type parameters, as explained in <xref:template-parameters>.

## Step 2. Generating the method body

The following template generates the body of a synchronous method that returns a value. The other five templates follow the same shape.

[!metalama-file GenerateProxyAspect.cs member="GenerateProxyAspect.NonVoidMethodTemplate"]

The template mixes compile-time and run-time code, as explained in <xref:template-compile-time>. The `argsType`, `writableParameters` and `arguments` variables are compile-time: they are evaluated while the aspect runs and leave no trace in the generated code. The call to `interceptorField.Value` and the local `Invoke` function are run-time code.

The local `Invoke` function reprojects the tuple elements onto the parameters of the intercepted method. `ExpressionFactory.Parse( "receivedArgs" )` builds an expression that refers to the parameter of the generated local function, and `argsType.TupleElements[p.Index].WithObject( ... )` builds the expression `receivedArgs.<name>` for each parameter.

## Step 3. Writing back out and ref arguments

An `out` parameter cannot be read before it is assigned, so the argument tuple cannot simply copy it. The aspect substitutes a default value for every `out` parameter when it builds the tuple.

[!metalama-file GenerateProxyAspect.cs member="GenerateProxyAspect.CreateTupleInstance"]

After the interceptor returns, the values that the interceptor may have changed must be copied from the tuple back into the parameters. Only `out` and `ref` parameters are copied back, because `in` and `ref readonly` parameters cannot be assigned. The copy happens in a `finally` block, so the values are written back even when the method throws.

The template omits the `try` and `finally` blocks entirely when the method has no `out` or `ref` parameter, so the common case costs nothing. The following interface, taken from the test suite of this example, exercises all four parameter modes.

[!metalama-file ../tests/Metalama.Samples.Proxy.Tests/OutRefParameters.cs]

Read the limitation below before relying on this part of the aspect.

## Step 4. Initializing the proxy

The instance constructor stores the interceptor and the intercepted object.

[!metalama-file GenerateProxyAspect.cs member="GenerateProxyAspect.Constructor"]

The `interceptor` parameter is declared by the template, while the `intercepted` parameter is added by the `buildConstructor` delegate in `BuildAspect`. Parameters declared by the template come first in the generated signature, which is why the generated constructor reads `CalculatorProxy( IInterceptor interceptor, ICalculator intercepted )`.

The static constructor fills the metadata fields. It is introduced with `AddInitializer` and `InitializerKind.BeforeTypeConstructor`, so the code runs before any other static initialization of the class.

[!metalama-file GenerateProxyAspect.cs member="GenerateProxyAspect.StaticConstructor"]

The `foreach` loop is compile-time: it runs while the aspect executes and produces one assignment per method. `item.Method.ToMethodInfo()` generates the reflection expression that resolves the method, which is the `typeof(ICalculator).GetMethod( ... )` call visible in the generated class.

## Applying the aspect from a fabric

Adding an assembly-level attribute for each interface is repetitive. A fabric can add the aspect to a set of interfaces instead, as explained in <xref:fabrics-adding-aspects>.

[!metalama-file FabricExtensions.cs]

The aspect targets the compilation, but the fabric selects interfaces, so the query cannot add the aspect directly. `WithTag` stores the selected interface, `Select( type => type.Compilation )` moves the query to the compilation, and the tag is read back in the `AddAspect` delegate to build the aspect instance.

A fabric then needs a single statement.

[!metalama-file ../tests/Metalama.Samples.Proxy.Tests/Fabric.cs]

When no name is given, the extension method removes the first character of the interface name and appends `Proxy`, so `ISomeInterface` yields `SomeInterfaceProxy`.

## Reporting unsupported return types

The aspect can generate an asynchronous method only for `Task`, `Task<TResult>`, `ValueTask` and `ValueTask<TResult>`. For any other awaitable return type it reports an error and skips the method, as explained in <xref:diagnostics>.

[!metalama-file DiagnosticDefinitions.cs]

## Limitations

A method with an `out` or `ref` parameter does not compile. The aspect builds the copy-back code described in step 3, and that code is correct once it is formatted, but the syntax the aspect hands to the compiler is rejected with error `CS1510`, reported as `LAMA0611`. The aspect tests do not detect this, because `src/metalamaTests.json` sets `FormatOutput`, so the test harness compiles the formatted code. Until this is fixed, restrict the interface to by-value parameters.

The aspect implements the methods of the interface, and only its methods. An interface that declares a property, an indexer or an event is not supported.

When a method has an `out` or `ref` parameter, the aspect uses the synchronous templates even if the method returns a task. The interceptor then receives the task as an ordinary return value and cannot await it, although the metadata still reports the return type as awaitable. This follows from passing the argument tuple by reference, which an asynchronous method cannot do.

The default naming rule in `FabricExtensions` removes the first character of the type name unconditionally. It gives the expected result for an interface named according to the usual convention, and a truncated name for any other.

> [!div class="see-also"]
> - <xref:introducing-types>
> - <xref:implementing-interfaces>
> - <xref:template-parameters>
> - <xref:fabrics-adding-aspects>
> - <xref:Metalama.Framework.Aspects.CompilationAspect>
> - <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceClass*>
