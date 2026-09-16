using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

[Inheritable]
public class TrackChangesAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // Implement the ISwitchableChangeTracking interface.
        builder.ImplementInterface(
            typeof(ISwitchableChangeTracking),
            OverrideStrategy.Ignore );

        // Override all writable fields and automatic properties.
        var fieldsOrProperties = builder.Target.FieldsAndProperties
            .Where( f => !f.IsImplicitlyDeclared &&
                         f.IsAutoPropertyOrField == true &&
                         f.Writeability == Writeability.All );

        foreach ( var fieldOrProperty in fieldsOrProperties )
        {
            builder.With( fieldOrProperty ).OverrideAccessors( null, nameof(this.OverrideSetter) );
        }
    }

    [InterfaceMember]
    public bool IsChanged { get; private set; }

    [InterfaceMember]
    public bool IsTrackingChanges { get; set; }

    [InterfaceMember]
    public void AcceptChanges() => this.IsChanged = false;

    [Introduce( WhenExists = OverrideStrategy.Ignore )]
    protected void OnChange()
    {
        if ( this.IsTrackingChanges )
        {
            this.IsChanged = true;
        }
    }

    [Template]
    private void OverrideSetter( dynamic? value )
    {
        if ( value != meta.Target.Property.Value )
        {
            meta.Proceed();

            this.OnChange();
        }
    }
}