using System.ComponentModel;

public interface ISwitchableChangeTracking : IRevertibleChangeTracking
{
    /// <summary>
    /// Gets or sets a value indicating whether the current object
    /// is tracking its changes.
    /// </summary>
    bool IsTrackingChanges { get; set; }
}