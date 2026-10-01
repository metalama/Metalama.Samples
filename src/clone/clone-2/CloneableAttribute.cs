using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Project;

[Inheritable]
[EditorExperience( SuggestAsLiveTemplate = true )]
public class CloneableAttribute : TypeAspect
{
    // [<snippet DiagnosticDefinitions>]
    private static readonly DiagnosticDefinition<(DeclarationKind, IFieldOrProperty)>
        _fieldOrPropertyCannotBeReadOnly =
            new(
                "CLONE01",
                Severity.Error,
                "The {0} '{1}' cannot be read-only because it is marked as a [Child]." );

    private static readonly DiagnosticDefinition<(DeclarationKind, IFieldOrProperty, IType)>
        _missingCloneMethod =
            new(
                "CLONE02",
                Severity.Error,
                "The {0} '{1}' cannot be a [Child] because its type '{2}' does not have a 'Clone' parameterless method." );

    private static readonly DiagnosticDefinition<IMethod> _cloneMethodMustBePublic =
        new(
            "CLONE03",
            Severity.Error,
            "The '{0}' method must be public or internal." );

    private static readonly DiagnosticDefinition<IProperty> _childPropertyMustBeAutomatic =
        new(
            "CLONE04",
            Severity.Error,
            "The property '{0}' cannot be a [Child] because is not an automatic property." );

    // [<endsnippet DiagnosticDefinitions>]

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // Verify child fields and properties.
        if ( !this.VerifyFieldsAndProperties( builder ) )
        {
            builder.SkipAspect();

            return;
        }

        // Introduce the Clone method.
        builder.IntroduceMethod(
            nameof(this.CloneImpl),
            whenExists: OverrideStrategy.Override,
            args: new { T = builder.Target },
            buildMethod: m =>
            {
                m.Name = "Clone";
                m.ReturnType = builder.Target;
            } );

        // Implement the ICloneable interface.
        builder.ImplementInterface(
            typeof(ICloneable),
            OverrideStrategy.Ignore );
    }

    private bool VerifyFieldsAndProperties( IAspectBuilder<INamedType> builder )
    {
        var success = true;

        // Verify that child fields are valid.
        foreach ( var fieldOrProperty in GetCloneableFieldsOrProperties( builder.Target ) )
        {
            // The field or property must be writable.
            if ( fieldOrProperty.Writeability != Writeability.All )
            {
                builder.Diagnostics.Report(
                    _fieldOrPropertyCannotBeReadOnly.WithArguments(
                        (
                            fieldOrProperty.DeclarationKind,
                            fieldOrProperty) ),
                    fieldOrProperty );

                success = false;
            }

            // If it is a field, it must be an automatic property.
            if ( fieldOrProperty is IProperty { IsAutoPropertyOrField: false } property )
            {
                builder.Diagnostics.Report(
                    _childPropertyMustBeAutomatic.WithArguments( property ),
                    property );

                success = false;
            }

            // The type of the field must be cloneable.
            void ReportMissingMethod()
            {
                builder.Diagnostics.Report(
                    _missingCloneMethod.WithArguments(
                        (fieldOrProperty.DeclarationKind,
                         fieldOrProperty,
                         fieldOrProperty.Type) ),
                    fieldOrProperty );
            }

            if ( fieldOrProperty.Type is not INamedType fieldType )
            {
                // The field type is an array, a pointer or another special type, which do not have a Clone method.
                ReportMissingMethod();
                success = false;
            }
            else
            {
                var cloneMethod = fieldType.AllMethods.OfName( "Clone" )
                    .SingleOrDefault( p => p.Parameters.Count == 0 );

                if ( cloneMethod == null )
                {
                    // There is no Clone method.
                    // It may be implemented by an aspect, but we don't have access to aspects on other types
                    // at design time.
                    if ( !MetalamaExecutionContext.Current.ExecutionScenario.IsDesignTime )
                    {
                        if ( !fieldType.BelongsToCurrentProject ||
                             !fieldType.Enhancements().HasAspect<CloneableAttribute>() )
                        {
                            ReportMissingMethod();
                            success = false;
                        }
                    }
                }
                else if ( cloneMethod.Accessibility is not (Accessibility.Public
                         or Accessibility.Internal) )
                {
                    // If we have a Clone method, it must be public.
                    builder.Diagnostics.Report(
                        _cloneMethodMustBePublic.WithArguments( cloneMethod ),
                        fieldOrProperty );

                    success = false;
                }
            }
        }

        return success;
    }

    private static IEnumerable<IFieldOrProperty> GetCloneableFieldsOrProperties( INamedType type )
        => type.FieldsAndProperties.Where( f =>
                                               f.Attributes.OfAttributeType( typeof(ChildAttribute) ).Any() );

    [Template]
    public virtual T CloneImpl<[CompileTime] T>()
    {
        // This compile-time variable will receive the expression representing the base call.
        // If we have a public Clone method, we will use it (this is the chaining pattern). Otherwise,
        // we will call MemberwiseClone (this is the initialization of the pattern).
        IExpression baseCall;

        if ( meta.Target.Method.IsOverride )
        {
            baseCall = (IExpression) meta.Base.Clone();
        }
        else
        {
            baseCall = (IExpression) meta.This.MemberwiseClone();
        }

        // Define a local variable of the same type as the target type.
        var clone = (T) baseCall.Value!;

        // Select cloneable fields.
        var cloneableFields = GetCloneableFieldsOrProperties( meta.Target.Type );

        foreach ( var field in cloneableFields )
        {
            // Check if we have a public method 'Clone()' for the type of the field.
            var fieldType = (INamedType) field.Type;

            field.WithObject( clone ).Value = meta.Cast( fieldType, field.Value?.Clone() );
        }

        return clone;
    }

    [InterfaceMember( IsExplicit = true )]
    [UsedImplicitly]
    private object Clone() => meta.This.Clone();
}