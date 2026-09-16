namespace Metalama.Samples.Clone.Tests.MissingOnChangeMethod;

[TrackChanges]
public class DerivedClass : BaseClass { }

public class BaseClass : ISwitchableChangeTracking
{
    public bool IsChanged { get; protected set; }

    public bool IsTrackingChanges { get; set; }

    public void AcceptChanges()
    {
        if ( this.IsTrackingChanges )
        {
            this.IsChanged = false;
        }
    }

    // Note that there is NO OnChange method.
}