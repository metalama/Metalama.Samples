using Metalama.Framework.Aspects;
using Metalama.Samples.Comparison3;

[assembly:
    AspectOrder(
        AspectOrderDirection.CompileTime,
        typeof(EqualityMemberAttribute),
        typeof(ImplementEquatableAttribute) )]