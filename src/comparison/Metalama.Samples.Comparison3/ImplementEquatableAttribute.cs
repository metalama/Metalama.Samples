using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;

namespace Metalama.Samples.Comparison3;

[Inheritable]
public class ImplementEquatableAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        // [<snippet FindFields>]
        // Identify the field and automatic properties that might be part of the comparison, look for custom attributes.
        var targetType = builder.Target;

        var fields = targetType.FieldsAndProperties.Where( f =>
                                                               f.IsAutoPropertyOrField == true
                                                               && f is { IsStatic: false, IsImplicitlyDeclared: false }
                                                               && f.Attributes.Any( typeof(EqualityMemberAttribute) ) )
            .ToList();

        // [<endsnippet FindFields>]

        // [<snippet ValidateFields>]
        // If there are no members, do not implement the aspect.
        if ( fields.Count == 0 )
        {
            // Write an error unless the aspect was applied through inheritance.
            if ( builder.AspectInstance.Predecessors[0].Kind != AspectPredecessorKind.Inherited )
            {
                builder.Diagnostics.Report( DiagnosticDefinitions.NoEqualityMemberError.WithArguments( targetType ) );
            }

            return;
        }

        // [<endsnippet ValidateFields>]

        // Find the base Equals method.
        IMethod? baseEqualsMethod = null;

        for ( var parent = builder.Target.BaseType;
              parent != null && parent.SpecialType != SpecialType.Object;
              parent = parent.BaseType )
        {
            baseEqualsMethod = parent.Methods
                .OfName( "Equals" )
                .SingleOrDefault( m => m.Parameters.Count == 1
                                       && m.Parameters[0].Type.Equals( m.DeclaringType )
                                       && m.IsAccessibleFrom( targetType ) );

            if ( baseEqualsMethod != null )
            {
                if ( !CheckMethodOverridable( baseEqualsMethod, targetType, builder ) )
                {
                    return;
                }

                break;
            }
        }

        // [<endsnippet FindBaseEquals>]

        // [<snippet FindBaseGetHashCode>]
        // Find the base GetHashCode method.
        var baseGetHashCodeMethod =
            targetType.BaseType?.AllMethods.OfName( nameof(object.GetHashCode) ).SingleOrDefault();

        if ( baseGetHashCodeMethod?.DeclaringType.Equals( SpecialType.Object ) == true )
        {
            // Do not call the GetHashCode method defined on System.Object because it returns
            // a hash of the reference, which is irrelevant to us.
            baseGetHashCodeMethod = null;
        }

        if ( !CheckMethodOverridable( baseGetHashCodeMethod, targetType, builder ) )
        {
            return;
        }

        // [<endsnippet FindBaseGetHashCode>]

        // Add the IEquatable interface to the type (interface members will be added lower).
        builder.ImplementInterface( TypeFactory.GetNamedType( typeof(IEquatable<>) ).WithTypeArguments( targetType ) );

        // Introduce the Equals methods.
        builder.IntroduceMethod(
            nameof(this.TypedEqualsTemplate),
            args: new { T = targetType, fields, baseEqualsMethod },
            buildMethod: m => m.IsVirtual = !targetType.IsSealed );

        builder.IntroduceMethod(
            nameof(this.UntypedEqualsTemplate),
            whenExists: OverrideStrategy.Override,
            args: new { T = targetType, fields } );

        // [<snippet IntroduceBaseTypeEquals>]
        if ( baseEqualsMethod != null )
        {
            builder.IntroduceMethod(
                nameof(this.BaseTypeEqualsTemplate),
                whenExists: OverrideStrategy.Override,
                args: new { TBase = baseEqualsMethod.DeclaringType, T = targetType } );
        }

        // Introduce the GetHashCode method.
        builder.IntroduceMethod(
            nameof(this.GetHashCodeTemplate),
            whenExists: OverrideStrategy.Override,
            args: new { T = targetType, fields, baseGetHashCodeMethod } );

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
    }

    private static bool CheckMethodOverridable(
        IMethod? method,
        INamedType targetType,
        IAspectBuilder<INamedType> builder )
    {
        if ( method != null && !method.IsOverridable() )
        {
            builder.Diagnostics.Report(
                DiagnosticDefinitions.BaseMethodMustBeVirtual.WithArguments( (method, targetType) ) );

            return false;
        }

        return true;
    }

    // Template for the top-level Equals(T) method.
    [Template( Name = "Equals" )]
    public bool TypedEqualsTemplate<[CompileTime] T>(
        T? other,
        IReadOnlyList<IFieldOrProperty> fields,
        IMethod? baseEqualsMethod )
    {
        // The following `if` is evaluated at compile time, so the block is only
        // emitted for reference types.
        if ( meta.Target.Type.IsReferenceType == true )
        {
            // [<snippet TypedEqualsTemplate_CallBaseMethod>]
            // Call the base strongly-typed Equals method, which typically has a parameter of the base type, but
            // is overridden in the current type by the BaseTypeEqualsTemplate template.
            if ( baseEqualsMethod != null )
            {
                if ( !baseEqualsMethod.WithOptions( InvokerOptions.Base ).Invoke( other ) )
                {
                    return false;
                }
            }

            // [<snippet TypedEqualsTemplate_CallBaseMethod>]
            else
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
        }

        // Compare fields of the current type one by one.
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

    // Templates for the new method hiding the base typed Equals method.
    [Template( Name = "Equals", IsSealed = true )]
    public bool BaseTypeEqualsTemplate<[CompileTime] TBase, [CompileTime] T>( TBase? other )
    {
        // First check for reference equality because this is very fast.
        if ( ReferenceEquals( meta.This, other ) )
        {
            return true;
        }

        return other is T typed && meta.This.Equals( typed );
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
    public int GetHashCodeTemplate( IReadOnlyList<IFieldOrProperty> fields, IMethod? baseGetHashCodeMethod )
    {
        var hashCode = default(HashCode);

        // [<snippet CallBaseGetHashCode>]
        if ( baseGetHashCodeMethod != null )
        {
            hashCode.Add( baseGetHashCodeMethod.WithOptions( InvokerOptions.Base ).Invoke() );
        }

        // [<endsnippet CallBaseGetHashCode>]

        foreach ( var field in fields )
        {
            var defaultComparer = TypeFactory.GetNamedType( typeof(EqualityComparer<>) )
                .WithTypeArguments( field.Type )
                .Properties["Default"];

            hashCode.Add( field.Value, defaultComparer.Value );
        }

        return hashCode.ToHashCode();
    }

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
}