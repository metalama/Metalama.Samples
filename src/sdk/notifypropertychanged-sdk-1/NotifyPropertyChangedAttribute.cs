using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.ComponentModel;

[Inheritable]
internal class NotifyPropertyChangedAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // Get the dependency graph.
        var dependencyGraph = DependencyHelper.GetPropertyDependencyGraph( builder.Target );

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
            builder.With( property )
                .OverrideAccessors(
                    null,
                    nameof(this.OverridePropertySetter),
                    new { dependencyGraph } );
        }
    }

    [InterfaceMember]
    public event PropertyChangedEventHandler? PropertyChanged;

    [Introduce( WhenExists = OverrideStrategy.Ignore )]
    protected void OnPropertyChanged( string name )
        => this.PropertyChanged?.Invoke( meta.This, new PropertyChangedEventArgs( name ) );

    [Template]
    private dynamic OverridePropertySetter(
        dynamic value,
        [CompileTime] Dictionary<string, List<string>> dependencyGraph )
    {
        if ( value != meta.Target.Property.Value )
        {
            meta.Proceed();

            // Notify change of the current properties.
            this.OnPropertyChanged( meta.Target.Property.Name );

            // Notify changes of other properties that depend on the current property.
            if ( dependencyGraph.TryGetValue(
                    meta.Target.Property.Name,
                    out var referencingProperties ) )
            {
                foreach ( var referencingProperty in referencingProperties )
                {
                    if ( referencingProperty != meta.Target.Property.Name )
                    {
                        this.OnPropertyChanged( referencingProperty );
                    }
                }
            }
        }

        return value;
    }
}