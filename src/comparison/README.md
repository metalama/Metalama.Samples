---
uid: sample-comparison
---
# Implementing equality comparison without boilerplate

As a .NET developer, you're likely familiar with the <xref:System.Object.Equals%2A> method in .NET. It's used to determine whether two instances are equivalent. Alongside this, there's <xref:System.Object.GetHashCode>, which generates a hash code often used for dictionary keys.

While the default .NET implementation is generally reasonable and works in most scenarios, there are cases where implementing the <xref:System.IEquatable%601> interface can be beneficial. However, it involves writing a lot of boilerplate code. In this series of articles, we will explore how to write an aspect that automatically generates this boilerplate during compilation.

As is often the case, equality comparison seems like a simple problem on the surface, but it's an intriguing rabbit hole to explore when it comes to customization and optimization. We'll start simple and add complexity progressively.

This series contains the following articles:

| Article | Description |
|---------|-------------|
| [Why and when to use a custom equality contract?](why.md) | This article discusses when to depart from the default .NET equality implementations. |
| [Step 1: Getting started - Basic implementation](Metalama.Samples.Comparison1/README.md) | This article walks you through a basic implementation including all fields and properties in the comparison, while omitting type inheritance. |
| [Step 2: Supporting type inheritance](Metalama.Samples.Comparison2/README.md) | This article adds type inheritance to the mix. |
| [Step 3: Hand-picking equality members](Metalama.Samples.Comparison3/README.md) | This article describes how to add an API so users can hand-pick fields and properties that must be part of the comparison. |
| [Step 4: Customization and optimizations](Metalama.Samples.Comparison4/README.md) | This article shows how to let the user choose a different `IEqualityComparer` instance and how to optimize the member evaluation order. |

At the end of this series, you'll be able to create aspects that generate code like the following examples:

[!metalama-files Metalama.Samples.Comparison4/Entity.cs Metalama.Samples.Comparison4/VersionedEntity.cs Metalama.Samples.Comparison4/Person.cs]
