using Metalama.Framework.Aspects;

[assembly:
    AspectOrder(
        AspectOrderDirection.RunTime,
        typeof(TrackChangesAttribute),
        typeof(NotifyPropertyChangedAttribute) )]