---
uid: sample-cache-3
level: 300
summary: "This document explains how to build a cache key using a custom attribute `[CacheKeyMember]` and an `ICacheKey` interface, detailing the design and implementation process."
keywords: "cache key, caching implementation"
created-date: 2023-04-06
modified-date: 2024-09-09
---

# Caching example, step 3: building the cache key

[!metalama-project-buttons .]

In the previous implementation of the aspect, the cache key came from an interpolated string that implicitly called
the `ToString` method for all parameters of the cached method. This approach is simplistic because it assumes that all
parameters have a suitable implementation of the `ToString` method: one that returns a distinct string for each unique
instance.

To alleviate this limitation, our objective is to make it sufficient for users of our framework to mark with
a `[CacheKeyMember]` custom attribute all fields or properties that should be part of the cache key. This is not a
trivial goal so let's first think about the design.

## Pattern design

First, we define an interface `ICacheKey`. When a type or struct implements this interface, we will
call `ICacheKey.ToCacheKey` instead of the `ToString` method:

[!metalama-file ICacheKey.cs]

We now need to think about an _implementation pattern_ for this interface, i.e., something that we can repeat for all
classes. The pattern needs to be _inheritable_, i.e., it should support the case when a class derives from a base class
that already implements `ICacheKey`, but the derived class adds a member to the cache key. The simplest pattern is to
always implement the following method:

```cs
protected virtual void BuildCacheKey( StringBuilder stringBuilder )
```

Each implementation of `BuildCacheKey` would first call the base implementation if any and then contribute its members
to the `StringBuilder`.

## Example code

To see the pattern in action, let's consider four classes `EntityKey`, `Entity`, `Invoice`, and `InvoiceVersion` that
can be part of a cache key, and a cacheable API `DatabaseFrontend`.

[!metalama-files EntityKey.cs Entity.cs Invoice.cs InvoiceVersion.cs DatabaseFrontend.cs links="false"]

> [!div class="see-also"]

## Pattern implementation

As we decided during the design phase, the public API of our cache key feature is the `[CacheKeyMember]` custom
attribute, which can be applied to fields or properties. The effect of this attribute needs to be the implementation of
the `ICacheKey` interface and the `BuildCacheKey` method. Because `CacheKeyMemberAttribute` is a field-or-property-level
attribute, and we want to perform a type-level transformation, we will use an internal helper aspect
called `GenerateCacheKeyAspect`.

The only action of the `CacheKeyMemberAttribute` aspect is then to provide the `GenerateCacheKeyAspect` aspect:

[!metalama-file CacheKeyMemberAttribute.cs]

The `BuildAspect` method of `CacheKeyMemberAttribute` calls
the <xref:Metalama.Framework.Aspects.AspectQueryExtensions.RequireAspect*> method for the declaring type. This method adds
an instance of the `GenerateCacheKeyAspect` if none has been added yet, so that if a class has several properties marked
with `[CacheKeyMember]`, a single instance of the `GenerateCacheKeyAspect` aspect will be added.

Let's now look at the implementation of `GenerateCacheKeyAspect`:

[!metalama-file GenerateCacheKeyAspect.cs]

The `BuildAspect` method of `GenerateCacheKeyAspect`
calls <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*> to add the `ICacheKey` interface to the
target type. The `whenExists` parameter is set to `Ignore`, which means that this call will just be ignored if the
target type or a base type already implements the interface.
The <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*> method requires the interface members to be
implemented by the aspect class and to be annotated with
the <xref:Metalama.Framework.Aspects.InterfaceMemberAttribute?text=[InterfaceMember]> custom attribute. Here, our only
member is `ToCacheKey`, which instantiates a `StringBuilder` and calls the `BuildCacheKey` method.

The `BuildCacheKey` aspect method is marked with
the <xref:Metalama.Framework.Aspects.IntroduceAttribute?text=[Introduce]> custom attribute, which means that Metalama
will add the method to the target type. The <xref:Metalama.Framework.Aspects.IntroduceAttribute.WhenExists> property
specifies what should happen when the type or a base type already defines the member: we choose to override the existing
implementation.

The first thing `BuildCacheKey` does is to execute the existing implementation if any, thanks to a call
to `meta.Proceed()`.

Secondly, the method finds all members that have the `CacheKeyMemberAttribute` aspect. Note that we are
using `property.Enhancements().HasAspect<CacheKeyMemberAttribute>()` and
not `f.Attributes.OfAttributeType(typeof(CacheKeyMemberAttribute)).Any()`. The first expression looks for aspects, while
the second one looks for custom attributes. What is the difference, if `CacheKeyMemberAttribute` is an aspect, anyway?
If the `CacheKeyMemberAttribute` aspect is programmatically added, using fabrics, for instance,
then `Enhancements().HasAspect` will see these new instances, while the `Attributes` collections will not.

Then, `BuildCacheKey` iterates through the members and emits a call to `stringBuilder.Append` for each member. When the
type of the member already implements `ICacheKey` or has an aspect of type `GenerateCacheKeyAspect` (i.e., _will_
implement `ICacheKey` after code transformation), we call `ICacheKey.ToCacheKey`. Otherwise, we call `ToString`. If the
member is `null`, we append just the `"null"` string.

Finally, the `CacheAttribute` aspect needs to be updated to take the `ICacheKey` interface into account. We must
consider the same four cases.

[!metalama-file CacheAttribute.cs]

## Ordering of Aspects

We now have three aspects in our solution. Because they are interdependent, their execution needs to be properly ordered
using a global <xref:Metalama.Framework.Aspects.AspectOrderAttribute>:

[!metalama-file MetalamaInfo.cs]

> [!div class="see-also"]
> <xref:child-aspects>
> <xref:ordering-aspects>



