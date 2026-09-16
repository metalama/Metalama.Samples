---
uid: sample-cache-4
level: 300
summary: "This document discusses generating cache keys for external types by introducing cache key builders and compile-time error reporting for unsupported types."
keywords: "cache key"
created-date: 2023-04-12
modified-date: 2024-09-09
---

# Caching example, step 4: cache key for external types

[!metalama-project-buttons .]

In the preceding article, we introduced the concept of generating cache keys for custom types by implementing
the `ICacheKey` interface. We created an aspect that implements this interface automatically for all the fields or
properties of a custom class annotated with the `[CacheKeyMember]` attribute.

However, two issues remain with this approach. Firstly, how do we handle types for which we don't have the source code?
Secondly, what if the user of this aspect tries to include an item whose type is not supported? We are now adding two
requirements to our aspect:

1. Add a mechanism to generate a cache key for externally-defined types, and
2. Report an error when the aspect's user attempts to include an unsupported type in the cache key.

## ICacheKeyBuilder

To address these challenges, we have introduced the concept of _cache key builders_ &mdash; objects capable of building
a
cache key for another object. We define the `ICacheKeyBuilder` interface as follows:

[!metalama-file ICacheKeyBuilder.cs]

The generic type parameter in the interface represents the relevant object type. The benefit of using a generic
parameter is performance: we can generate the cache key without boxing value-typed values into an `object`.

For instance, here is an implementation for `byte[]`:

[!metalama-file ByteArrayCacheKeyBuilder.cs]

## Compile-time API

To enable compile-time reporting of errors when attempting to include an unsupported type in the cache key, we need a
compile-time configuration API for the caching aspects. We accomplish this via a concept named _hierarchical options_,
which is explained in more detail in <xref:aspect-configuration>. We define a new compile-time class, `CachingOptions`,
to map types to their respective builders. It implements the <xref:Metalama.Framework.Options.IHierarchicalOptions`1>
for all levels where the options can be defined, i.e. the whole compilation, namespace, or type. The class is designed
as immutable and represents an incremental _change_ in configuration compared to its base level, but without knowing its
base configuration. Because of this unusual requirements, designing aspect options can be complex.

Here is the top-level option class:

[!metalama-file CachingOptions.cs]

The class relies on <xref:Metalama.Framework.Options.IncrementalKeyedCollection> to represent a change in the
collection. Items in these collections are represented by the `CacheBuilderRegistration` class.

[!metalama-file CacheBuilderRegistration.cs]

Configuring the caching API using a fabric is straightforward:

[!metalama-file Fabric.cs]

For those unfamiliar with the term, fabrics are compile-time types whose `AmendProject` method executes before any
aspect. The `AmendProject` method acts as a compile-time entry point, triggered solely by its existence, much
like `Program.Main`, but at compile time. Refer to <xref:fabrics> for additional information.

## ICacheKeyBuilderProvider

At run time, it is convenient to abstract the process of obtaining `ICacheKeyBuilder` instances with a provider pattern.
We can achieve this by defining the `ICacheKeyBuilderProvider` interface.

[!metalama-file ICacheKeyBuilderProvider.cs]

Note that the `new()` constraint on the generic parameter allows for a trivial implementation of the class.

The implementation of `ICacheKeyBuilderProvider` should be pulled from the dependency injection container. To allow
cache key objects to be instantiated independently from the dependency container, we update the `ICacheKey` interface to
receive the provider from the caller:

[!metalama-file ICacheKey.cs]

## Generating the cache key item expression

The logic to generate the expression that gets the cache key of an object has now grown in complexity. It includes
support for three cases plus null-handling.

* Implicit call to `ToString`.
* Call to `ICacheKeyBuilderProvider.GetCacheKeyBuilder`.
* Call to `ICacheKey.ToCacheKey`.

It is now easier to build the expression with <xref:Metalama.Framework.Code.SyntaxBuilders.ExpressionBuilder> rather
than with a template. We have moved this logic to `CachingOptions`.

[!metalama-file CachingOptionsExtensions.cs member="CachingOptionsExtensions.TryGetCacheKeyExpression"]

The <xref:Metalama.Framework.Code.SyntaxBuilders.ExpressionBuilder> class essentially acts as a `StringBuilder` wrapper.
We can add any text to an `ExpressionBuilder`, as long as it can be parsed back into a valid C# expression.

## Reporting errors for unsupported types

We report an error whenever an unsupported type is used as a parameter of a cached method, or when it is used as a type
for a field or property annotated with `[CacheKeyMember]`.

To achieve this, we add the following code to `CachingOptions`:

[!metalama-file CachingOptionsExtensions.cs member="CachingOptionsExtensions.VerifyCacheKeyMember"]

The first line defines an error kind. Metalama requires the <xref:Metalama.Framework.Diagnostics.DiagnosticDefinition>
to be defined in a static field or property. Then, if the type of the `expression` is invalid, this error is reported
for that property or parameter. To learn more about reporting errors, see <xref:diagnostics>.

This method needs to be reported from the `BuildAspect` method of the `CacheAttribute` and `GenerateCacheKeyAspect`
aspect classes. We cannot report errors from template methods because templates are typically not executed at design
time unless we are using the _preview_ feature.

However, a limitation prevents us from detecting unsupported types at design time. When Metalama runs inside the editor,
at design time, it doesn't execute all aspects for all files at every keystroke, but only does so for the files that
have been edited, plus all files containing the ancestor types. Therefore, at design time, your aspect receives a
_partial_ compilation. It can still see all the types in the project, but it doesn't see the aspects that have been
applied to these types.

So, when `CachingOptions.VerifyCacheKeyMember` evaluates `Enhancements().HasAspect<GenerateCacheKeyAspect>()` at design
time, the expression does not yield an accurate result. Therefore, we can only run this method when we have a complete
compilation, i.e., at compile time.

To verify parameters, we need to include this code in the `CacheAttribute` aspect class:

[!metalama-file CacheAttribute.cs member="CacheAttribute.BuildAspect"]

## Aspects in action

The aspects can be applied to some business code as follows:

[!metalama-files BlobId.cs DatabaseFrontend.cs links="false"]

> [!div class="see-also"]
> <xref:aspect-configuration>
> <xref:fabrics>
> <xref:diagnostics>



