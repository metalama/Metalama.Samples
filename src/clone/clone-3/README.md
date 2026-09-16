---
uid: sample-clone-3
summary: "The document explains enhancing a `Cloneable` aspect to allow custom user logic for cloning external types and collections by implementing a `CloneMembers` method."
keywords: "Cloneable aspect"
created-date: 2023-04-21
modified-date: 2024-09-09
---

# Clone example, step 3: allowing handmade customizations

[!metalama-project-buttons .]

In the previous articles, we built a `Cloneable` aspect that worked well with simple classes and one-to-one
relationships. But what if we need to support external types for which we cannot add a `Clone` method, or one-to-many
relationships, such as collection fields?

Ideally, we would build a pluggable cloning service for external types, as we did for caching key builders of external
types (see <xref:sample-cache-4>) and supply cloners for system collections. But before that, an even better strategy is
to design an extension point that the aspect's users can use when our aspect has limitations. How can we allow the
aspect's users to inject their custom logic?

We will let users add their custom logic after the aspect-generated logic by allowing them to supply a method with the
following signature, where `T` is the current type:

```cs
private void CloneMembers(T clone)
```

The aspect will inject its logic before the user's implementation.

Let's see this pattern in action. In this new example, the `Game` class has a one-to-many relationship with the `Player`
class. The cloning of the collection is implemented manually.

[!metalama-files Game.cs GameSettings.cs]

## Aspect implementation

Here is the updated `CloneableAttribute` class:

[!metalama-file CloneableAttribute.cs]

We added the following code in the `BuildAspect` method:

[!metalama-file CloneableAttribute.cs marker="AddCloneMembers"]

The template for the `CloneMembers` method is as follows:

[!metalama-file CloneableAttribute.cs member="CloneableAttribute.CloneMembers"]

As you can see, we moved the logic that clones individual fields to this method. We call `meta.Proceed()` _last_, so
hand-written code is executed after aspect-generated code and can fix whatever gap the aspect left.

## Summary

We updated the aspect to add an extensibility mechanism allowing the user to implement scenarios that lack genuine
support by the aspect. The problem with this approach is that users may easily forget that they have to supply
a `private void CloneMembers(T clone)` method. To remedy this issue, we will provide them with suggestions in the code
refactoring menu.



