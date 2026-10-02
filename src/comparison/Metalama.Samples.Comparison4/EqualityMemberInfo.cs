using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Samples.Comparison4;

[CompileTime]
internal record EqualityMemberInfo( IFieldOrProperty Field, EqualityMemberAttribute Aspect );