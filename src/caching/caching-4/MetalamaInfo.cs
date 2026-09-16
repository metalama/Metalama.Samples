using Metalama.Framework.Aspects;

// Aspects are executed at compile time in the inverse order than the one given here.
// It is important that aspects are executed in the given order because they rely on each other:
//  - CacheKeyMemberAttribute provides GenerateCacheKeyAspect, so CacheKeyMemberAttribute should run before GenerateCacheKeyAspect.
//  - CacheAttribute relies on CacheKeyMemberAttribute, so CacheAttribute should run after.

[assembly:
    AspectOrder(
        AspectOrderDirection.RunTime,
        typeof(CacheAttribute),
        typeof(GenerateCacheKeyAspect),
        typeof(CacheKeyMemberAttribute) )]