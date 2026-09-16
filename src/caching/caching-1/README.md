---
uid: sample-cache-1
level: 250
summary: "This document explains the basics of implementing caching using an aspect-oriented approach, comparing non-cached and cached code, and detailing the infrastructure and aspect code."
keywords: "caching, cache key, MemoryCache"
created-date: 2023-04-06
modified-date: 2024-09-09
---

# Caching example, step 1: getting started

[!metalama-project-buttons .]

At first glance, caching appears simple. The aspect generates the cache key, performs a cache lookup, and returns the
result if it exists. If it doesn't exist, the method runs, and the return value gets stored in the cache. The following
example compares the source code, without caching logic, to the transformed code, with caching logic.

[!metalama-compare Calculator.cs]

## Infrastructure Code

The aspect relies on the `ICache` interface, which has only two methods used by the aspect.

[!metalama-file ICache.cs]

A typical implementation of this interface would use  <xref:System.Runtime.Caching.MemoryCache>.

## Aspect Code

The aspect itself is straightforward:

[!metalama-file CacheAttribute.cs]

As usual, the aspect class inherits the abstract class <xref:Metalama.Framework.Aspects.OverrideMethodAspect>, which, in
turn, derives from the <xref:System.Attribute?text=System.Attribute> class. This makes `CacheAttribute` a custom
attribute. The <xref:Metalama.Framework.Aspects.OverrideMethodAspect.OverrideMethod*> method acts like a _template_.
Most of its code gets injected into the target method, the one to which we add
the `[ReportAndSwallowExceptionsAttribute]` custom attribute. Inside
the <xref:Metalama.Framework.Aspects.OverrideMethodAspect.OverrideMethod*> implementation, the call to `meta.Proceed()`
has a unique meaning. When the aspect applies to the target, the call to `meta.Proceed()` stands in for the original
implementation, with a few syntactic alterations that capture the return value.

The `CacheKeyBuilder` class hides the complexity of creating the cache key. It is a compile-time class.

[!metalama-file CacheKeyBuilder.cs]

As its name suggests, the <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder> class helps build
interpolated strings. The `meta.Target` property exposes the context into which the template
applies.  `meta.Target.Type` is the current type, `meta.Target.Method` is the current method, and so on.
The `GetCachingKey` method creates an interpolated string for the current context. Parameters get represented at compile
time using the <xref:Metalama.Framework.Code.IParameter> interface.
The <xref:Metalama.Framework.Code.IExpression.Value?text=parameter.Value> expression returns a `dynamic` object that
represents a _run-time_ expression. In this case, it's the name of the parameter.

Upon receiving the <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder> instance from
the `CacheKeyBuilder` class, the aspect converts it to a run-time interpolated string by calling
the <xref:Metalama.Framework.Code.SyntaxBuilders.ExpressionBuilderExtensions.ToValue*> method. This method returns
a `dynamic` object representing the interpolated string. It can be cast to a `string`, and we use it as the caching key.


> [!div class="see-also"]
> - <xref:template-overview>
> - <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder>
> - <xref:template-compile-time>
> - <xref:dynamic-typing>



