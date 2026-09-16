---
uid: sample-dirty-4
summary: "This article explains how to implement the ability to revert changes in objects using the `IRevertibleChangeTracking` interface in .NET."
keywords: "IRevertibleChangeTracking, reverting changes, RejectChanges method, .NET, AcceptChanges method"
created-date: 2023-04-18
modified-date: 2024-09-09
---

# Change Tracking example, step 4: reverting changes

[!metalama-project-buttons .]

In this article, we will implement the ability to revert the object to the last-accepted version. The .NET Framework
exposes this ability as the <xref:System.ComponentModel.IRevertibleChangeTracking> interface. It adds a
new <xref:System.ComponentModel.IRevertibleChangeTracking.RejectChanges*> method. This method must revert any changes
performed since the last call to the <xref:System.ComponentModel.IChangeTracking.AcceptChanges*> method.

We need to duplicate each field or automatic property: one copy will contain the _current_ value, and the second will
contain the _accepted_ value. The <xref:System.ComponentModel.IChangeTracking.AcceptChanges*> method copies the current
values to the accepted ones, while the <xref:System.ComponentModel.IRevertibleChangeTracking.RejectChanges*> method
copies the accepted values to the current ones.

Let's see this pattern in action:

[!metalama-files Comment.cs ModeratedComment.cs]

## Aspect implementation

Here is the complete code of the new version of the `TrackChanges` aspect:

[!metalama-file TrackChangesAttribute.cs]

Let's focus on the following part of the `BuildAspect` method for the moment.

[!metalama-file TrackChangesAttribute.cs marker="BuildDictionary"]

First, the method introduces new fields into the type for each mutable field or automatic property using
the <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceField*> method. For details about this practice,
see <xref:introducing-members>.

Note that we are building the `introducedFields` dictionary, which maps the current-value field or property to the
accepted-value field. This dictionary will be passed to
the <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*> call as a _tag_. The collection of tags is
an
anonymous object. For more details about this technique, see <xref:sharing-state-with-advice>.

The field dictionary is read from the implementation of `AcceptChanges` and `RejectChanges`:

[!metalama-file TrackChangesAttribute.cs member="TrackChangesAttribute.AcceptChanges"]

[!metalama-file TrackChangesAttribute.cs member="TrackChangesAttribute.RejectChanges"]

As you can see, the `(Dictionary<IFieldOrProperty, IField>) meta.Tags["IntroducedFields"]` expression gets
the `IntroducedFields` tag, which was passed to
the <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*> method. We cast it back to its original
type
and iterate it. We use the <xref:Metalama.Framework.Code.IExpression.Value> property to generate the run-time expression
that represents the field or property. In the `AcceptChanges` method, we copy the current values to the accepted ones
and do the opposite in the `RejectChanges` method.

> [!div class="see-also"]
> <xref:introducing-members>
> <xref:sharing-state-with-advice>



