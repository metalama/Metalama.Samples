using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using System.Globalization;

[Inheritable]
public class TrackChangesAttribute : TypeAspect
{
    private static readonly DiagnosticDefinition<INamedType> _mustHaveOnChangeMethod = new(
        "MY001",
        Severity.Error,
        $"The '{nameof(ISwitchableChangeTracking)}' interface is implemented manually on type '{{0}}', but the type does not have an '{nameof(OnChange)}()' method." );

    private static readonly DiagnosticDefinition _onChangeMethodMustBeProtected = new(
        "MY002",
        Severity.Error,
        $"The '{nameof(OnChange)}()' method must be have the 'protected' accessibility." );

    private static readonly DiagnosticDefinition<IMethod> _onPropertyChangedMustBeVirtual = new(
        "MY003",
        Severity.Error,
        "The '{0}' method must be virtual." );

    [InterfaceMember( WhenExists = InterfaceMemberOverrideStrategy.Ignore )]
    public bool IsChanged { get; private set; }

    [InterfaceMember( WhenExists = InterfaceMemberOverrideStrategy.Ignore )]
    public bool IsTrackingChanges
    {
        get => meta.This._isTrackingChanges;
        set
        {
            if ( meta.This._isTrackingChanges != value )
            {
                meta.This._isTrackingChanges = value;

                var onPropertyChanged = this.GetOnPropertyChangedMethod( meta.Target.Type );

                if ( onPropertyChanged != null )
                {
                    onPropertyChanged.Invoke( nameof(this.IsTrackingChanges) );
                }

                if ( value )
                {
                    this.AcceptChanges();
                }
            }
        }
    }

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // Select fields and automatic properties that can be changed.
        var fieldsOrProperties = builder.Target.FieldsAndProperties
            .Where( f =>
                        !f.IsImplicitlyDeclared && f.Writeability == Writeability.All &&
                        f.IsAutoPropertyOrField == true )
            .ToArray();

        // [<snippet BuildDictionary>]
        var introducedFields = new Dictionary<IFieldOrProperty, IField>();

        // Create a field for each mutable field or property. These fields
        // will contain the accepted values.
        foreach ( var fieldOrProperty in fieldsOrProperties )
        {
            var upperCaseName = fieldOrProperty.Name.TrimStart( '_' );

            upperCaseName =
                upperCaseName.Substring( 0, 1 ).ToUpper( CultureInfo.InvariantCulture ) +
                upperCaseName.Substring( 1 );

            var acceptedField =
                builder.IntroduceField(
                    "_accepted" + upperCaseName,
                    fieldOrProperty.Type );

            introducedFields[fieldOrProperty] = acceptedField.Declaration;
        }

        // Implement the ISwitchableChangeTracking interface.         
        var implementInterfaceResult = builder.ImplementInterface(
            typeof(ISwitchableChangeTracking),
            OverrideStrategy.Ignore,
            new { IntroducedFields = introducedFields } );

        // [<endsnippet BuildDictionary>]

        // If the type already implements ISwitchableChangeTracking, it must have a protected method called OnChanged, without parameters, otherwise
        // this is a contract violation, so we report an error.
        if ( implementInterfaceResult.Outcome == AdviceOutcome.Ignore )
        {
            var onChangeMethod = builder.Target.AllMethods.OfName( nameof(this.OnChange) )
                .SingleOrDefault( m => m.Parameters.Count == 0 );

            if ( onChangeMethod == null )
            {
                builder.Diagnostics.Report( _mustHaveOnChangeMethod.WithArguments( builder.Target ) );
            }
            else if ( onChangeMethod.Accessibility != Accessibility.Protected )
            {
                builder.Diagnostics.Report( _onChangeMethodMustBeProtected );
            }
        }
        else
        {
            builder.IntroduceField( "_isTrackingChanges", typeof(bool) );
        }

        // Override all writable fields and automatic properties.
        // If the type has an OnPropertyChanged method, we assume that all properties
        // and fields already call it, and we hook into OnPropertyChanged instead of
        // overriding each setter.
        var onPropertyChanged = this.GetOnPropertyChangedMethod( builder.Target );

        if ( onPropertyChanged == null )
        {
            // If the type has an OnPropertyChanged method, we assume that all properties
            // and fields already call it, and we hook into OnPropertyChanged instead of
            // overriding each setter.
            foreach ( var fieldOrProperty in fieldsOrProperties )
            {
                builder.With( fieldOrProperty )
                    .OverrideAccessors(
                        null,
                        nameof(this.OverrideSetter) );
            }
        }
        else if ( onPropertyChanged.DeclaringType.Equals( builder.Target ) )
        {
            builder.With( onPropertyChanged ).Override( nameof(this.OnPropertyChanged) );
        }
        else if ( implementInterfaceResult.Outcome == AdviceOutcome.Ignore )
        {
            // If we have an OnPropertyChanged method but the type already implements ISwitchableChangeTracking,
            // we assume that the type already hooked the OnPropertyChanged method, and
            // there is nothing else to do.
        }
        else
        {
            // If the OnPropertyChanged method was defined in a base class, but not overridden
            // in the current class, and if we implement ISwitchableChangeTracking ourselves,
            // then we need to override OnPropertyChanged.

            if ( !onPropertyChanged.IsVirtual )
            {
                builder.Diagnostics.Report( _onPropertyChangedMustBeVirtual.WithArguments( onPropertyChanged ) );
            }
            else
            {
                builder.IntroduceMethod(
                    nameof(this.OnPropertyChanged),
                    whenExists: OverrideStrategy.Override );
            }
        }
    }

    private IMethod? GetOnPropertyChangedMethod( INamedType type )
        => type.AllMethods
            .OfName( "OnPropertyChanged" )
            .SingleOrDefault( m => m.Parameters.Count == 1 );

    [InterfaceMember]
    public virtual void AcceptChanges()
    {
        if ( meta.Target.Method.IsOverride )
        {
            meta.Proceed();
        }
        else
        {
            this.IsChanged = false;
        }

        var introducedFields =
            (Dictionary<IFieldOrProperty, IField>) meta.Tags["IntroducedFields"]!;

        foreach ( var field in introducedFields )
        {
            field.Value.Value = field.Key.Value;
        }
    }

    [InterfaceMember]
    public virtual void RejectChanges()
    {
        if ( meta.Target.Method.IsOverride )
        {
            meta.Proceed();
        }
        else
        {
            this.IsChanged = false;
        }

        var introducedFields =
            (Dictionary<IFieldOrProperty, IField>) meta.Tags["IntroducedFields"]!;

        foreach ( var field in introducedFields )
        {
            field.Key.Value = field.Value.Value;
        }
    }

    [Introduce( WhenExists = OverrideStrategy.Ignore )]
    protected void OnChange()
    {
        if ( this.IsChanged == false )
        {
            this.IsChanged = true;

            var onPropertyChanged = this.GetOnPropertyChangedMethod( meta.Target.Type );

            if ( onPropertyChanged != null )
            {
                onPropertyChanged.Invoke( nameof(this.IsChanged) );
            }
        }
    }

    [Template]
    private void OverrideSetter( dynamic? value )
    {
        meta.Proceed();

        if ( value != meta.Target.Property.Value )
        {
            this.OnChange();
        }
    }

    [Template]
    protected virtual void OnPropertyChanged( string name )
    {
        meta.Proceed();

        if ( name is not (nameof(this.IsChanged) or nameof(this.IsTrackingChanges)) )
        {
            this.OnChange();
        }
    }
}