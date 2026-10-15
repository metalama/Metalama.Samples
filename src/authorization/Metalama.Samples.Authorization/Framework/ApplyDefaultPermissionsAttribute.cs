using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Samples.Authorization;

// ApplyDefaultPermissions adds RequiresPermission aspects, so RequiresPermission must be processed after it.
[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(RequiresPermissionAttribute), typeof(ApplyDefaultPermissionsAttribute) )]

namespace Metalama.Samples.Authorization;

/// <summary>
/// Aspect that, applied to a class, adds <see cref="RequiresPermissionAttribute"/> with the default permissions
/// of <see cref="AuthorizationOptions"/> to the fields, properties and (optionally) methods of the class that
/// do not have it yet. The aspect is inherited by derived classes.
/// </summary>
[Inheritable]
[AttributeUsage( AttributeTargets.Class )]
public sealed class ApplyDefaultPermissionsAttribute : TypeAspect
{
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
                    method => method.MethodKind == MethodKind.Default
                              && !OverridesObjectMethod( method )
                              && method.Enhancements().GetOptions<AuthorizationOptions>() is { DefaultExecutePermission: not null } options
                              && IsSecuredByDefault( method, options ) ) )
            .AddAspectIfEligible( _ => new RequiresPermissionAttribute() );
    }

    private static bool IsSecuredByDefault( IMember member, AuthorizationOptions options )
    {
        if ( member.IsImplicitlyDeclared || member.IsStatic )
        {
            return false;
        }

        // An explicit attribute wins over the defaults.
        if ( member.Attributes.Any( typeof(RequiresPermissionAttribute) ) )
        {
            return false;
        }

        if ( options.IsDefaultSecuringEnabled == false )
        {
            return false;
        }

        return member.Accessibility switch
        {
            Accessibility.Public => true,
            Accessibility.Private => false,
            _ => options.SecureNonPublicMembers == true
        };
    }

    // ToString, Equals and GetHashCode are called by infrastructure code, such as exception messages
    // and collections, so they are never secured by default.
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
