using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.ComponentModel;

[Inheritable]
internal class NotifyPropertyChangedAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // Implement the interface.
        builder.ImplementInterface(
            typeof(INotifyPropertyChanged),
            OverrideStrategy.Ignore );

        // Override the property setters.
        foreach ( var property in builder.Target.Properties.Where( p =>
                                                                       p is
                                                                       {
                                                                           IsAbstract: false,
                                                                           Writeability: Writeability.All
                                                                       } ) )
        {
            builder.With( property ).OverrideAccessors( null, nameof(this.OverridePropertySetter) );
        }
    }

    [Introduce]
    private static readonly Dictionary<string, string[]> _propertyDependencies =
        DependencyHelper.GetPropertyDependencyGraph( meta.Target.Type );

    [InterfaceMember]
    public event PropertyChangedEventHandler? PropertyChanged;

    [Introduce( WhenExists = OverrideStrategy.Override )]
    protected virtual void OnPropertyChanged( string name )
    {
        if ( meta.Target.Method.OverriddenMethod != null )
        {
            // If we override OnPropertyChanged from the base class, invoke it.
            meta.Proceed();
        }
        else
        {
            // Otherwise, invoke the event.
            this.PropertyChanged?.Invoke( meta.This, new PropertyChangedEventArgs( name ) );
        }

        if ( _propertyDependencies.TryGetValue( name, out var dependentProperties ) )
        {
            foreach ( var dependentProperty in dependentProperties )
            {
                this.OnPropertyChanged( dependentProperty );
            }
        }
    }

    [Template]
    private dynamic OverridePropertySetter( dynamic value )
    {
        if ( value != meta.Target.Property.Value )
        {
            meta.Proceed();

            // Notify change of the current properties.
            this.OnPropertyChanged( meta.Target.Property.Name );
        }

        return value;
    }
}