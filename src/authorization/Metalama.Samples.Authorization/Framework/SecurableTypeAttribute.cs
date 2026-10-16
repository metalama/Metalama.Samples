using Metalama.Extensions.Validation;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Metalama.Samples.Authorization;

// The order of execution at run time, from the outermost aspect to the innermost:
// - RequiresPermission checks the permission of the member before anything else runs.
// - GrantsPermissions grants permissions only after this check has passed, so the grant cannot satisfy it.
// - SecurableType adds RequiresPermission aspects, so it must be processed first at build time, which is the
//   reverse of the run-time order.
[assembly: AspectOrder(
    AspectOrderDirection.RunTime,
    typeof(RequiresPermissionAttribute),
    typeof(GrantsPermissionsAttribute),
    typeof(SecurableTypeAttribute) )]

namespace Metalama.Samples.Authorization;

/// <summary>
/// Aspect that makes a class securable: it adds <see cref="RequiresPermissionAttribute"/> with the default
/// permissions of <see cref="AuthorizationOptions"/> to the fields, properties and methods of the class that do not
/// have it yet, and validates that no member is left unsecured.
/// </summary>
/// <remarks>
/// <para>The aspect is applied to the <see cref="ISecurable"/> interface and inherited by every class that implements
/// it, and by their derived classes. It can also be applied directly to a class.</para>
/// <para>The caller of a constructor has unlimited rights on the new object while it is initialized: the aspect
/// starts the initialization before the code of the constructors, and ends it after the object initializer (see
/// <see cref="SecurityContext.BeginInitialization"/>).</para>
/// <para>The aspect secures by default: every field and property, every public method, and every explicit interface
/// implementation, gets a check.
/// A member is excluded only explicitly, with <c>[AuthorizationOptions( IsDefaultSecuringEnabled = false )]</c>.
/// After all aspects have been applied, a validator reports a warning for every exposed member that still has no
/// check and was not excluded explicitly, for instance a static member, an indexer or an event, which the aspects
/// cannot secure.</para>
/// </remarks>
[Inheritable]
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface )]
public sealed class SecurableTypeAttribute : TypeAspect
{
    private static readonly DiagnosticDefinition<(IDeclaration Member, INamedType Type)> _unsecuredMemberWarning = new(
        "AUTH001",
        Severity.Warning,
        "'{0}' is exposed by the secured type '{1}' but has no permission check. " +
        "Add [RequiresPermission], or exclude the member explicitly with [AuthorizationOptions( IsDefaultSecuringEnabled = false )]." );

    private static readonly DiagnosticDefinition<(IDeclaration Member, INamedType Type)> _staticMemberWarning = new(
        "AUTH002",
        Severity.Warning,
        "'{0}' is a static member of the secured type '{1}'. A static member has no instance on which to check a permission. " +
        "Move it to another type, or exclude it explicitly with [AuthorizationOptions( IsDefaultSecuringEnabled = false )]." );

    private static readonly DiagnosticDefinition<(IDeclaration Member, IType Type)> _mutableCollectionWarning = new(
        "AUTH003",
        Severity.Warning,
        "'{0}' returns the collection type '{1}', which the getter cannot secure: a caller who can read it may change its " +
        "content without the write permission. Declare it as IList<T>, ICollection<T>, IReadOnlyList<T>, " +
        "IReadOnlyCollection<T> or IEnumerable<T>, initialized with a SecurableList<T>, or as an immutable type." );

    // On an interface, the aspect does nothing itself: it is inherited by the classes that implement the interface.
    // The aspect is useful only on a securable type: the checks are made on the current instance.
    public override void BuildEligibility( IEligibilityBuilder<INamedType> builder )
    {
        builder.ExceptForInheritance().MustNotBeInterface();
        builder.MustSatisfy( type => type.IsConvertibleTo( typeof(ISecurable) ), type => $"{type} must implement ISecurable" );
    }

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // The object is being initialized from the first constructor that runs until the end of the object
        // initializer. The call sites of the constructors call Initialize after the object initializer.
        builder.AddInitializer( nameof(this.BeginInitialization), InitializerKind.BeforeInstanceConstructor );
        builder.AddInitializer( nameof(this.EndConstruction), InitializerKind.AfterLastInstanceConstructor );
        builder.AddInitializer( nameof(this.EndInitialization), InitializerKind.AfterObjectInitializer );

        builder.Outbound
            .SelectMany(
                type => type.FieldsAndProperties.Where(
                    member => IsSecuredByDefault( member, member.Enhancements().GetOptions<AuthorizationOptions>() ) ) )
            .AddAspectIfEligible( _ => new RequiresPermissionAttribute() );

        builder.Outbound
            .SelectMany(
                type => type.Methods.Where(
                    method => IsOrdinaryMethod( method )
                              && !OverridesObjectMethod( method )
                              && IsSecuredByDefault( method, method.Enhancements().GetOptions<AuthorizationOptions>() ) ) )
            .AddAspectIfEligible( _ => new RequiresPermissionAttribute() );

        // Verifies the final code, after all aspects, so that it sees the checks added by any aspect.
        builder.Outbound
            .SelectMany( type => type.Members().Where( IsValidated ) )
            .Validate( ValidateMember, new DeclarationValidationOptions { Time = DeclarationValidationTime.AfterAllAspects } );
    }

    private static bool IsSecuredByDefault( IMember member, AuthorizationOptions options )
        => !member.IsStatic
           && !member.Attributes.Any( typeof(RequiresPermissionAttribute) ) // An explicit attribute wins over the defaults.
           && IsExposed( member, options );

    // Determines whether a member must have a check, unless it is excluded explicitly.
    private static bool IsExposed( IMember member, AuthorizationOptions options )
    {
        if ( member.IsImplicitlyDeclared || options.IsDefaultSecuringEnabled == false || IsInitializationMethod( member ) )
        {
            return false;
        }

        // An explicit interface implementation is private, but any caller reaches it through the interface.
        if ( member.IsExplicitInterfaceImplementation )
        {
            return true;
        }

        // The fields and properties hold the state of the object, so they are secured whatever their accessibility:
        // a member of the class must not change or read the state without the permission. A private static member
        // has no instance to secure, and is not exposed.
        if ( member is IFieldOrProperty )
        {
            return !member.IsStatic || member.Accessibility != Accessibility.Private;
        }

        return member.Accessibility switch
        {
            Accessibility.Public => true,
            Accessibility.Private => false,
            _ => options.SecureNonPublicMembers == true
        };
    }

    // The methods that Metalama introduces for the initializers, such as the Initialize method that the call sites of
    // the constructors call. They end the initialization of the object, so a check would fail for every caller who
    // cannot write the object.
    private static bool IsInitializationMethod( IMember member )
        => member is IMethod { Origin.Kind: DeclarationOriginKind.Aspect, Name: "Initialize" or "OnConstructed" };

    private static bool IsValidated( IMember member )
        => member switch
        {
            // An abstract member has no body to check; each override is validated instead.
            { IsAbstract: true } => false,

            IMethod method => IsOrdinaryMethod( method ) && !OverridesObjectMethod( method ),
            IConstructor => false,
            _ => true
        };

    private static void ValidateMember( DeclarationValidationContext context )
    {
        var member = (IMember) context.Declaration;

        var options = member switch
        {
            IMethod method => method.Enhancements().GetOptions<AuthorizationOptions>(),
            IFieldOrProperty fieldOrProperty => fieldOrProperty.Enhancements().GetOptions<AuthorizationOptions>(),
            IIndexer indexer => indexer.Enhancements().GetOptions<AuthorizationOptions>(),
            IEvent @event => @event.Enhancements().GetOptions<AuthorizationOptions>(),
            _ => null
        };

        // The aspect supports methods, fields and properties. An indexer or an event never has a check.
        var hasCheck = member switch
        {
            IMethod method => method.Enhancements().HasAspect<RequiresPermissionAttribute>(),
            IFieldOrProperty fieldOrProperty => HasCheck( fieldOrProperty ),
            _ => false
        };

        if ( options == null || !IsExposed( member, options ) )
        {
            return;
        }

        // A secured getter protects the reference, not the content of a mutable collection.
        if ( member is IFieldOrProperty { Type: var type } && CollectionTypes.Classify( type ) == CollectionKind.Unsecurable )
        {
            context.Diagnostics.Report( _mutableCollectionWarning.WithArguments( (member, type) ) );
        }

        if ( !hasCheck )
        {
            var warning = member.IsStatic ? _staticMemberWarning : _unsecuredMemberWarning;
            context.Diagnostics.Report( warning.WithArguments( (member, member.DeclaringType) ) );
        }
    }

    // To secure a field, Metalama turns it into a property. The aspect is on the field, and the final code model
    // shows the property, so both are checked.
    private static bool HasCheck( IFieldOrProperty fieldOrProperty )
        => fieldOrProperty.Enhancements().HasAspect<RequiresPermissionAttribute>()
           || fieldOrProperty is IProperty { OriginalField: { } field } && field.Enhancements().HasAspect<RequiresPermissionAttribute>()
           || fieldOrProperty is IField { OverridingProperty: { } property } && property.Enhancements().HasAspect<RequiresPermissionAttribute>();

    [Template]
    private void BeginInitialization() => SecurityContext.BeginInitialization( meta.This );

    [Template]
    private void EndConstruction() => SecurityContext.EndConstruction( meta.This );

    [Template]
    private void EndInitialization() => SecurityContext.EndInitialization( meta.This );

    // Explicit interface implementations have their own method kind; accessors, operators and finalizers are excluded.
    private static bool IsOrdinaryMethod( IMethod method )
        => method.MethodKind is MethodKind.Default or MethodKind.ExplicitInterfaceImplementation;

    // ToString, Equals and GetHashCode are called by infrastructure code, such as collections and loggers,
    // so they are never secured by default.
    private static bool OverridesObjectMethod( IMethod method )
    {
        for ( var baseMethod = method.OverriddenMethod; baseMethod != null; baseMethod = baseMethod.OverriddenMethod )
        {
            if ( baseMethod.DeclaringType.SpecialType == SpecialType.Object )
            {
                return true;
            }
        }

        return false;
    }
}
