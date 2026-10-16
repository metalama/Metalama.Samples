using Metalama.Extensions.Validation;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;

namespace Metalama.Samples.Authorization;

/// <summary>
/// Aspect that grants permissions while a method, a property accessor or a constructor runs, after the permission
/// check of the member has passed. On a method, a property or a constructor, the permissions are granted on the
/// current instance (<c>this</c>). On a parameter, they are granted on the argument. Use it for an aggregate that the
/// subject may see although it is computed from details that the subject may not see, for instance a total computed
/// from secured lines, and for a constructor that initializes secured members of the new object.
/// </summary>
/// <remarks>
/// <para>The member or the parameter must have a permission check (<see cref="RequiresPermissionAttribute"/>, explicit
/// or added by default): otherwise any caller would get the granted permissions. A validator reports an error when it
/// has none. A constructor is the exception: it grants permissions on an object that it creates, and that no caller
/// can access before the constructor returns.</para>
/// <para>The grant runs inside the check: the aspect order makes <see cref="RequiresPermissionAttribute"/> the outer
/// aspect, so the grant cannot satisfy the check of its own member. The permission checks of the parameters also run
/// before the grant.</para>
/// <para>The grant applies to one object only, and only until the member returns. Accesses to other objects are
/// checked as usual. It applies to the code that the member calls, and to the tasks that the member starts, because
/// the grant flows with the logical call. It takes precedence over the policy, including its denials.</para>
/// <para>The aspect is inherited, like <see cref="RequiresPermissionAttribute"/>: an override of a member that grants
/// permissions grants them too, so that a derived class keeps the behavior of the base member.</para>
/// </remarks>
[Inheritable]
[AttributeUsage( AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Parameter )]
public sealed class GrantsPermissionsAttribute : Aspect, IAspect<IMethod>, IAspect<IProperty>, IAspect<IConstructor>, IAspect<IParameter>
{
    private static readonly DiagnosticDefinition<IDeclaration> _uncheckedMemberError = new(
        "AUTH004",
        Severity.Error,
        "'{0}' grants permissions but has no permission check, so any caller would get them. " +
        "Add [RequiresPermission], or remove [GrantsPermissions]." );

    private readonly string[] _permissions;

    /// <summary>
    /// Grants permissions on the current instance, or on the argument of a parameter, while the member runs.
    /// </summary>
    /// <param name="permissions">The names of the granted permissions.</param>
    public GrantsPermissionsAttribute( params string[] permissions )
    {
        this._permissions = permissions;
    }

    public void BuildEligibility( IEligibilityBuilder<IMethod> builder )
    {
        builder.MustNotBeStatic();

        // An abstract member has no body, but the aspect can still be put on it to grant in all overrides.
        builder.ExceptForInheritance().MustNotBeAbstract();
        builder.DeclaringType().MustBeConvertibleTo( typeof(ISecurable) );

        // The body of an iterator runs when the result is enumerated, after the member has returned.
        builder.MustSatisfy( method => method.GetIteratorInfo().IsIteratorMethod != true, method => $"{method} must not be an iterator" );
    }

    public void BuildEligibility( IEligibilityBuilder<IProperty> builder )
    {
        builder.MustNotBeStatic();
        builder.ExceptForInheritance().MustNotBeAbstract();
        builder.DeclaringType().MustBeConvertibleTo( typeof(ISecurable) );

        // An automatic property has no code that could use the grant.
        builder.MustSatisfy( property => property.IsAutoPropertyOrField != true, property => $"{property} must not be an automatic property" );
    }

    public void BuildEligibility( IEligibilityBuilder<IConstructor> builder )
    {
        builder.MustNotBeStatic();
        builder.DeclaringType().MustBeConvertibleTo( typeof(ISecurable) );
    }

    public void BuildEligibility( IEligibilityBuilder<IParameter> builder )
    {
        builder.MustNotBeReturnParameter();
        builder.MustBeReadable();
        builder.Type().MustBeConvertibleTo( typeof(ISecurable) );

        // The grant wraps the body of the method.
        builder.DeclaringMember().MustBeInstanceOfType( typeof(IMethod) );
        builder.DeclaringMember().ExceptForInheritance().MustNotBeAbstract();
        builder.DeclaringMember()
            .MustSatisfy(
                member => ((IMethod) member).GetIteratorInfo().IsIteratorMethod != true,
                member => $"{member} must not be an iterator" );
    }

    public void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        builder.Override( nameof(this.OverrideMethod), args: new { permissions = this._permissions } );
        ValidateLater( builder );
    }

    public void BuildAspect( IAspectBuilder<IProperty> builder )
    {
        var property = builder.Target;

        builder.OverrideAccessors(
            property.GetMethod != null ? (GetterTemplateSelector) nameof(this.GetTemplate) : default,
            property.SetMethod != null ? nameof(this.SetTemplate) : null,
            args: new { permissions = this._permissions } );

        ValidateLater( builder );
    }

    // The constructor creates the object, so no check is required before the grant. The grant starts after the call to
    // the base constructor or to another constructor of the class, and ends before the object is returned to the caller.
    public void BuildAspect( IAspectBuilder<IConstructor> builder )
        => builder.Override( nameof(this.OverrideConstructor), args: new { permissions = this._permissions } );

    public void BuildAspect( IAspectBuilder<IParameter> builder )
    {
        var parameter = builder.Target;

        builder.With( (IMethod) parameter.DeclaringMember! )
            .Override( nameof(this.OverrideMethodForParameter), args: new { parameter, permissions = this._permissions } );

        ValidateLater( builder );
    }

    // The check can be added by another aspect, for instance SecurableType, so the validator runs after all aspects.
    private static void ValidateLater<T>( IAspectBuilder<T> builder )
        where T : class, IDeclaration
        => builder.Outbound.Validate(
            context =>
            {
                var hasCheck = context.Declaration switch
                {
                    IMethod method => method.Enhancements().HasAspect<RequiresPermissionAttribute>(),
                    IProperty property => property.Enhancements().HasAspect<RequiresPermissionAttribute>(),
                    IParameter parameter => parameter.Enhancements().HasAspect<RequiresPermissionAttribute>(),
                    _ => false
                };

                if ( !hasCheck )
                {
                    context.Diagnostics.Report( _uncheckedMemberError.WithArguments( context.Declaration ) );
                }
            },
            new DeclarationValidationOptions { Time = DeclarationValidationTime.AfterAllAspects } );

    [Template]
    private dynamic? OverrideMethod( [CompileTime] string[] permissions )
    {
        using ( SecurityContext.Grant( meta.This, meta.RunTime( permissions ) ) )
        {
            return meta.Proceed();
        }
    }

    [Template]
    private void OverrideConstructor( [CompileTime] string[] permissions )
    {
        using ( SecurityContext.Grant( meta.This, meta.RunTime( permissions ) ) )
        {
            meta.Proceed();
        }
    }

    [Template]
    private dynamic? OverrideMethodForParameter( [CompileTime] IParameter parameter, [CompileTime] string[] permissions )
    {
        // A null argument has nothing to grant on: Grant accepts null and grants nothing.
        using ( SecurityContext.Grant( parameter.Value, meta.RunTime( permissions ) ) )
        {
            return meta.Proceed();
        }
    }

    [Template]
    private dynamic? GetTemplate( [CompileTime] string[] permissions )
    {
        using ( SecurityContext.Grant( meta.This, meta.RunTime( permissions ) ) )
        {
            return meta.Proceed();
        }
    }

    [Template]
    private void SetTemplate( dynamic? value, [CompileTime] string[] permissions )
    {
        using ( SecurityContext.Grant( meta.This, meta.RunTime( permissions ) ) )
        {
            meta.Proceed();
        }
    }
}
