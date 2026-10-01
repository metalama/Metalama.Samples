using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Samples.Comparison4;

[assembly:
    AspectOrder(
        AspectOrderDirection.CompileTime,
        typeof(EqualityMemberAttribute),
        typeof(ImplementEquatableAttribute) )]

namespace Metalama.Samples.Comparison4;

// [<snippet Ordering>]
public class EqualityMemberAttribute : FieldOrPropertyAspect
{
    protected const int DefaultOrder = 1000;

    public EqualityMemberAttribute( int order = DefaultOrder )
    {
        this.Order = order;
    }

    public int Order { get; }

    internal virtual int GetCost( IFieldOrProperty field )
    {
        // TODO: Base on benchmarks.

        return field.Type.SpecialType switch
        {
            SpecialType.None => 10,
            SpecialType.Object => 10,
            SpecialType.Void => 0,
            SpecialType.Boolean => 1,
            SpecialType.Char => 1,
            SpecialType.SByte => 1,
            SpecialType.Byte => 1,
            SpecialType.Int16 => 1,
            SpecialType.UInt16 => 1,
            SpecialType.Int32 => 1,
            SpecialType.UInt32 => 1,
            SpecialType.Int64 => 1,
            SpecialType.UInt64 => 1,
            SpecialType.Decimal => 2,
            SpecialType.Single => 2,
            SpecialType.Double => 2,
            SpecialType.String => 5,

            _ => field.Type switch
            {
                { TypeKind: TypeKind.Struct or TypeKind.Class } and INamedType { IsRecord: false }
                    when HasEqualsImplementation( (INamedType) field.Type ) => 10,
                { TypeKind: TypeKind.Struct or TypeKind.Class } and INamedType { IsRecord: true } => 20,
                { TypeKind: TypeKind.Struct } => 200,
                { TypeKind: TypeKind.Class } => 1,
                _ => 100
            }
        };

        bool HasEqualsImplementation( INamedType type )
            => type.AllMethods.OfName( "Equals" ).Any( m => m.Parameters.Count == 1 )
               || HasEqualsAspect( type );

        // TODO: The following will not work reliably if the target type is processed after the current type.
        // However, the impact is limited to performance.
        bool HasEqualsAspect( INamedType type )
            => !type.DeclaringAssembly.IsExternal
               && type.Enhancements().HasAspect<ImplementEquatableAttribute>();
    }

    // [<endsnippet Ordering>]

    public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
    {
        base.BuildAspect( builder );

        builder.With( builder.Target.DeclaringType ).RequireAspect<ImplementEquatableAttribute>();
    }

    public override void BuildEligibility( IEligibilityBuilder<IFieldOrProperty> builder )
    {
        base.BuildEligibility( builder );

        builder.MustNotBeStatic();
        builder.MustBeExplicitlyDeclared();
    }

    protected internal virtual IExpression GetComparerExpression( IFieldOrProperty field )
    {
        return TypeFactory.GetNamedType( typeof(EqualityComparer<>) )
            .WithTypeArguments( field.Type )
            .Properties["Default"];
    }
}