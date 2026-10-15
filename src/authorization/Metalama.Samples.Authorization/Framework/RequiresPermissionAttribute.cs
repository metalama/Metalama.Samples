using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Samples.Authorization;

/// <summary>
/// Aspect that requires the current subject to have a permission before a method executes,
/// before a field or property is read or written, or on the argument passed to a parameter.
/// </summary>
/// <remarks>
/// <para>On a method or a property, the permission is required on the current instance (<c>this</c>).
/// On a parameter, it is required on the argument, which lets a method check the objects it receives.</para>
/// <para>When the attribute does not name a permission, the defaults of <see cref="AuthorizationOptions"/> apply.</para>
/// <para>The aspect is inherited: an override of a secured virtual member is secured too, so a derived class
/// cannot skip the check by not calling the base implementation.</para>
/// </remarks>
[Inheritable]
[AttributeUsage( AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter )]
public sealed class RequiresPermissionAttribute : Aspect, IAspect<IMethod>, IAspect<IFieldOrProperty>, IAspect<IParameter>
{
    private readonly string? _permission;
    private readonly string? _writePermission;

    /// <summary>
    /// Requires the permissions configured by <see cref="AuthorizationOptions"/>.
    /// </summary>
    public RequiresPermissionAttribute() { }

    /// <summary>
    /// Requires a permission to execute a method, to receive an argument, or to read and write a field or property.
    /// </summary>
    public RequiresPermissionAttribute( string permission )
    {
        this._permission = permission;
    }

    /// <summary>
    /// Requires different permissions to read and to write a field or property.
    /// </summary>
    public RequiresPermissionAttribute( string readPermission, string writePermission )
    {
        this._permission = readPermission;
        this._writePermission = writePermission;
    }

    public void BuildEligibility( IEligibilityBuilder<IMethod> builder )
    {
        builder.MustNotBeStatic();

        // An abstract member has no body, but the aspect can still be put on it to secure all overrides.
        builder.ExceptForInheritance().MustNotBeAbstract();
    }

    public void BuildEligibility( IEligibilityBuilder<IFieldOrProperty> builder )
    {
        builder.MustNotBeStatic();
        builder.ExceptForInheritance().MustNotBeAbstract();
    }

    public void BuildEligibility( IEligibilityBuilder<IParameter> builder )
    {
        builder.MustNotBeReturnParameter();
        builder.MustBeReadable();

        builder.Type()
            .MustSatisfy(
                t => t.IsReferenceType == true,
                t => $"{t} must be a reference type" );
    }

    public void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        var options = builder.Target.Enhancements().GetOptions<AuthorizationOptions>();
        var permission = this._permission ?? options.DefaultExecutePermission ?? PermissionNames.Write;

        builder.Override(
            nameof(this.OverrideMethod),
            args: new { permission, member = GetMemberName( builder.Target ) } );
    }

    public void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
    {
        var target = builder.Target;
        var options = target.Enhancements().GetOptions<AuthorizationOptions>();

        // A single permission applies to both reading and writing.
        var readPermission = this._permission ?? options.DefaultReadPermission ?? PermissionNames.Read;
        var writePermission = this._writePermission ?? this._permission ?? options.DefaultWritePermission ?? PermissionNames.Write;

        var secureGetter = target.GetMethod != null;

        // Private setters are implementation details: the class changes its own state through them,
        // typically from a method that is itself secured.
        var secureSetter = target.Writeability is Writeability.All or Writeability.InitOnly
                           && target.SetMethod is { Accessibility: not Accessibility.Private };

        if ( !secureGetter && !secureSetter )
        {
            builder.SkipAspect();

            return;
        }

        builder.OverrideAccessors(
            secureGetter ? (GetterTemplateSelector) nameof(this.GetTemplate) : default,
            secureSetter ? nameof(this.SetTemplate) : null,
            args: new
            {
                readPermission,
                writePermission,
                member = GetMemberName( target ),

                // A list returned by a secured getter is wrapped, so that changing it demands the write permission.
                wrapCollection = CollectionTypes.Classify( target.Type ) == CollectionKind.Wrappable
            } );
    }

    public void BuildAspect( IAspectBuilder<IParameter> builder )
    {
        var parameter = builder.Target;
        var options = parameter.Enhancements().GetOptions<AuthorizationOptions>();
        var permission = this._permission ?? options.DefaultReadPermission ?? PermissionNames.Read;
        var member = $"{GetMemberName( (IMember) parameter.DeclaringMember! )}({parameter.Name})";

        builder.AddContract( nameof(this.ValidateArgument), args: new { permission, member } );
    }

    private static string GetMemberName( IMember member )
    {
        if ( member is IConstructor )
        {
            return member.DeclaringType.Name;
        }

        // The name of an explicit interface implementation includes the namespace of the interface: keep only the
        // interface name, for instance Document.IArchivable.Archive.
        var name = member.Name;

        if ( member.IsExplicitInterfaceImplementation )
        {
            var lastDot = name.LastIndexOf( '.' );
            name = name.Substring( name.LastIndexOf( '.', lastDot - 1 ) + 1 );
        }

        return $"{member.DeclaringType.Name}.{name}";
    }

    [Template]
    private dynamic? OverrideMethod( [CompileTime] string permission, [CompileTime] string member )
    {
        AccessGuard.Demand( meta.This, permission, member, AccessKind.Execute );

        return meta.Proceed();
    }

    [Template]
    private dynamic? GetTemplate(
        [CompileTime] string readPermission,
        [CompileTime] string writePermission,
        [CompileTime] string member,
        [CompileTime] bool wrapCollection )
    {
        AccessGuard.Demand( meta.This, readPermission, member, AccessKind.Read );

        if ( wrapCollection )
        {
            return SecuredCollection.Wrap( meta.Proceed(), meta.This, writePermission, member );
        }

        return meta.Proceed();
    }

    [Template]
    private void SetTemplate(
        dynamic? value,
        [CompileTime] string readPermission,
        [CompileTime] string writePermission,
        [CompileTime] string member,
        [CompileTime] bool wrapCollection )
    {
        AccessGuard.Demand( meta.This, writePermission, member, AccessKind.Write );
        meta.Proceed();
    }

    [Template]
    private void ValidateArgument( dynamic? value, [CompileTime] string permission, [CompileTime] string member )
    {
        // A null argument has nothing to secure; nullability is a separate concern. Testing a copy
        // instead of the parameter keeps the parameter's nullable state intact for the rest of the method.
        object? argument = value;

        if ( argument != null )
        {
            AccessGuard.Demand( argument, permission, member, AccessKind.Execute );
        }
    }
}
