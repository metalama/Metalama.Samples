---
uid: sample-cache
summary: "How to cache the output of expensive methods with an aspect, instead of writing the caching boilerplate by hand."
keywords: "cache C#, cache .NET, caching C#, caching .NET, aspect"
created-date: 2023-04-06
modified-date: 2024-09-09
---

# Implementing caching without boilerplate

Caching stores the output of an expensive method in memory, so that subsequent calls with the same arguments return the stored result instead of executing the method again.

Only cache pure methods: methods that always return the same output for the same input, and that change no state outside themselves. If you cache a method that has side effects, those side effects silently stop happening on a cache hit.

Caching a method by hand takes a lot of boilerplate. An aspect removes it.

The hard part of caching is generating a cache key reliably. This series presents strategies for building one.

> [!INFO]
> The objective of this series is didactic. For a production-ready and battle-tested implementation of caching, use the `Metalama.Patterns.Caching.Aspects` package. See <xref:caching> for details.

## In this series

| Article | Description |
| ------- | ----------- |
| [Getting started](caching-1/README.md) | Presents a first working version of the caching aspect. |
| [Enforcing eligibility](caching-2/README.md) | Makes the aspect report an error for `void` methods or methods with `out` or `ref` parameters. |
| [Building the cache key](caching-3/README.md) | Explains how to add a `[CacheKeyMember]` aspect to make the specified field or property part of the cache key for that type. |
| [Supporting external types](caching-4/README.md) | Shows how to build a caching key that includes external types. |



