using Metalama.Extensions.Validation;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Samples.Authorization;

// ApplyDefaultPermissions adds RequiresPermission aspects, so RequiresPermission must be processed after it.
[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(RequiresPermissionAttribute), typeof(ApplyDefaultPermissionsAttribute) )]

namespace Metalama.Samples.Authorization;

/// <summary>
/// Aspect that, applied to a class, adds <see cref="RequiresPermissionAttribute"/> with the default permissions
/// of <see cref="AuthorizationOptions"/> to the fields, properties and methods of the class that do not have it
/// yet. The aspect is inherited by derived classes.
/// </summary>
/// <remarks>
/// The aspect secures by default: every public member, and every explicit interface implementation, gets a check.
/// A member is excluded only explicitly, with <c>[AuthorizationOptions( IsDefaultSecuringEnabled = false )]</c>.
/// After all aspects have been applied, a validator reports a warning for every exposed member that still has no
/// check and was not excluded explicitly, for instance a static member, an indexer or an event, which the aspects
/// cannot secure.
/// </remarks>
[Inheritable]
[AttributeUsage( AttributeTargets.Class )]
public sealed class ApplyDefaultPermissionsAttribute : TypeAspect
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
        "'{0}' exposes the mutable collection type '{1}': any caller who can read it can change its content without " +
        "the write permission. Declare it as IList<T> or ICollection<T>, which the getter wraps in a guarded view, or as a read-only type." );

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
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
        if ( member.IsImplicitlyDeclared || options.IsDefaultSecuringEnabled == false )
        {
            return false;
        }

        // An explicit interface implementation is private, but any caller reaches it through the interface.
        if ( member.IsExplicitInterfaceImplementation )
        {
            return true;
        }

        return member.Accessibility switch
        {
            Accessibility.Public => true,
            Accessibility.Private => false,
            _ => options.SecureNonPublicMembers == true
        };
    }

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
        if ( member is IFieldOrProperty { Type: var type } && CollectionTypes.Classify( type ) == CollectionKind.Mutable )
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
