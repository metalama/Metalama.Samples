namespace Metalama.Samples.Clone.Tests.OnChangeMethodNotProtected;

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

    // Note that the OnChange method is private and not protected.
    private void OnChange() => this.IsChanged = true;
}