using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Samples.Comparison1;

// [<snippet ImplementEquatableAttribute>]
public class ImplementEquatableAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

// [<endsnippet ImplementEquatableAttribute>]

        var targetType = builder.Target;

        // [<snippet IdentifyFields>]
        // Identify the field and automatic properties that will be part of the comparison.
        var fields = targetType.FieldsAndProperties.Where( f =>
                                                               f.IsAutoPropertyOrField == true && f is
                                                                   { IsStatic: false, IsImplicitlyDeclared: false } )
            .OrderBy( f => f.Name )
            .ToList();

        // [<endsnippet IdentifyFields>]

        // [<snippet ImplementInterface>]
        // Add the IEquatable interface to the type (members will be added lower).
        builder.ImplementInterface( TypeFactory.GetNamedType( typeof(IEquatable<>) ).WithTypeArguments( targetType ) );

        // [<endsnippet ImplementInterface>]

        // [<snippet IntroduceTypedEquals>]
        // Introduce the Equals(T) methods.
        builder.IntroduceMethod( nameof(this.TypedEqualsTemplate), args: new { T = targetType, fields } );

        // [<endsnippet IntroduceTypedEquals>]

        // [<snippet IntroduceUntypedEquals>]
        // Introduce the Equals(object) methods.
        builder.IntroduceMethod(
            nameof(this.UntypedEqualsTemplate),
            whenExists: OverrideStrategy.Override,
            args: new { T = targetType, fields } );

        // [<endsnippet IntroduceUntypedEquals>]

        // [<snippet IntroduceGetHashCode>]
        // Introduce the GetHashCode method.
        builder.IntroduceMethod(
            nameof(this.GetHashCodeTemplate),
            whenExists: OverrideStrategy.Override,
            args: new { T = targetType, fields } );

        // [<endsnippet IntroduceGetHashCode>]

        // [<snippet IntroduceOperators>]
        // Introduce the operators.
        builder.IntroduceMethod(
            nameof(this.EqualityOperatorTemplate),
            args: new { T = targetType },
            buildMethod: m =>
            {
                m.OperatorKind = OperatorKind.Equality;
            } );

        builder.IntroduceMethod(
            nameof(this.InequalityOperatorTemplate),
            args: new { T = targetType },
            buildMethod: m =>
            {
                m.OperatorKind = OperatorKind.Inequality;
            } );

        // [<endsnippet IntroduceOperators>]
    }

    // Template for the Equals(T) method.
    [Template( Name = "Equals" )]
    public bool TypedEqualsTemplate<[CompileTime] T>( T? other, IReadOnlyList<IFieldOrProperty> fields )
    {
        // The following `if` is evaluated at compile time, so the block is only
        // emitted for reference types.
        if ( meta.Target.Type.IsReferenceType == true )
        {
            if ( other == null )
            {
                return false;
            }

            if ( ReferenceEquals( meta.This, other ) )
            {
                return true;
            }
        }

        // Compare all fields.
        foreach ( var field in fields )
        {
            var defaultComparer = TypeFactory.GetNamedType( typeof(EqualityComparer<>) )
                .WithTypeArguments( field.Type )
                .Properties["Default"];

            if ( !defaultComparer.Value!.Equals( field.Value, field.WithObject( other ).Value ) )
            {
                return false;
            }
        }

        return true;
    }

    // Template for the Equals(object) method.
    [Template( Name = "Equals" )]
    public bool UntypedEqualsTemplate<[CompileTime] T>( object? other )
    {
        // If we have a reference type, first check for reference equality because this is very fast.
        if ( meta.Target.Type.IsReferenceType == true )
        {
            if ( ReferenceEquals( meta.This, other ) )
            {
                return true;
            }
        }

        return other is T typed && meta.This.Equals( typed );
    }

    // Template for the GetHashCode method.
    [Template( Name = "GetHashCode" )]
    public int GetHashCodeTemplate( IReadOnlyList<IFieldOrProperty> fields )
    {
        var hashCode = default(HashCode);

        foreach ( var field in fields )
        {
            var defaultComparer = TypeFactory.GetNamedType( typeof(EqualityComparer<>) )
                .WithTypeArguments( field.Type )
                .Properties["Default"];

            hashCode.Add( field.Value, defaultComparer.Value );
        }

        return hashCode.ToHashCode();
    }

    // [<snippet OperatorTemplates>]
    // Template for the == operator.
    [Template]
    public bool EqualityOperatorTemplate<[CompileTime] T>( T a, T b )
    {
        if ( meta.Target.Type.IsReferenceType == true )
        {
            return (a == null && b == null) || (a != null && a.Equals( b ));
        }
        else
        {
            return a!.Equals( b );
        }
    }

    // Template for the != operator.
    [Template]
    public bool InequalityOperatorTemplate<[CompileTime] T>( T a, T b )
    {
        if ( meta.Target.Type.IsReferenceType == true )
        {
            return ((a == null) ^ (b == null)) || (a != null && !a.Equals( b ));
        }
        else
        {
            return !a!.Equals( b );
        }
    }

    // [<endsnippet OperatorTemplates>]
}