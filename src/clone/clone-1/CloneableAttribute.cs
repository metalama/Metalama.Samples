using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

[Inheritable]
[EditorExperience( SuggestAsLiveTemplate = true )]
public class CloneableAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // [<snippet BuildAspect1>]
        builder.ImplementInterface(
            typeof(ICloneable),
            OverrideStrategy.Ignore );

        // [<endsnippet BuildAspect1>]

        // [<snippet BuildAspect2>]
        builder.IntroduceMethod(
            nameof(this.CloneImpl),
            whenExists: OverrideStrategy.Override,
            args: new { T = builder.Target },
            buildMethod: m => m.Name = "Clone" );

        // [<endsnippet BuildAspect2>]
    }

    [InterfaceMember( IsExplicit = true )]
    [UsedImplicitly]
    private object Clone() => meta.This.Clone();

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
        var cloneableFields =
            meta.Target.Type.FieldsAndProperties.Where( f => f.Attributes.OfAttributeType( typeof(ChildAttribute) )
                                                            .Any() );

        foreach ( var field in cloneableFields )
        {
            // Check if we have a public method 'Clone()' for the type of the field.
            var fieldType = (INamedType) field.Type;

            field.WithObject( clone ).Value = meta.Cast( fieldType, field.Value?.Clone() );
        }

        return clone;
    }
}