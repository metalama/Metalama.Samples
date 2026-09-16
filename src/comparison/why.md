---
uid: sample-comparison-why
---

# Why implement an equality comparison aspect?

Before we start, let's examine why and when it is beneficial to use an equality comparison aspect, as opposed to using the default .NET or C# implementation.

## Default implementation

In .NET, the default approach to equality comparison varies depending on the type: `class`, `struct`, and `record` each have their own strategy.

- **`class`**: By default, two class instances are _never_ considered equal. When you compare two `class` instances, only the _references_ are compared, equivalent to calling the <xref:System.Object.ReferenceEquals%2A> method.

- **`struct`**: The default .NET implementation checks the values of all fields and automatic properties, using the <xref:System.Object.Equals(System.Object)> method for both `class` and `struct` fields. However, <xref:System.Object.GetHashCode> is invoked for classes but not always for structs. This can lead to inconsistencies between <xref:System.Object.Equals%2A> and <xref:System.Object.GetHashCode> in edge cases where a struct `A` has a field of type `B`, `B` is a `struct` with a custom equality implementation, and not all fields of `B` are identity members.

- **`record`**: All fields and automatic properties, whether of `class` or `struct` types, are compared using <xref:System.Collections.Generic.EqualityComparer%601.Default>, where `T` is the field type. This means `record` types perform a deep comparison by default. The strongly-typed <xref:System.Object.Equals%2A> and custom <xref:System.Object.GetHashCode> methods are used in both cases.

## Advantages of a custom implementation

### For `struct` types

- **Performance**: You can achieve a significant performance boost with a custom equality implementation, often by two orders of magnitude. If you frequently compare custom structs, a custom implementation is essential.
- **Fixing edge cases**: As mentioned earlier, there's a slight chance you might encounter the edge cases described above.
- **Different string comparison**: The default comparison mode for strings is <xref:System.StringComparison.Ordinal>. If you need a different mode, like case-insensitive, you'll need to provide a custom equality comparison implementation.

### For `class` types

- **Different equality behaviors**: Sometimes, altering the default equality behavior is desirable for certain object families. For example, if you have an `Entity` class with `EntityType` and `EntityId` fields, along with other data fields, you might want the default comparison to consider only these two fields, ignoring others. This means two distinct instances with the same `EntityType` and `EntityId` but different data fields would be considered equal.

### For `record` types

- In general, overriding the default equality implementation should be done cautiously. The identity contract is a core feature of `record` types, unlike `class` types, and modifying this behavior can contradict the principle of least surprise.
- **Ignoring irrelevant fields**: A valid reason for overriding might be to ignore an irrelevant record field. For instance, you might have an `ObjectId` field used only for debugging, not stored or serialized over the network, and shouldn't affect equality comparison. In such cases, overriding the equality implementation is justified.
- **Different string comparison**: As with structs, you'll need a custom equality contract if you want a different string comparison mode than <xref:System.StringComparison.Ordinal>.

## Why not manually implement the custom implementation?

Considering that the default implementation can be less than ideal, you might think about manually implementing the <xref:System.IEquatable%601> interface, including the operators.

There are two problems with this approach:

1. It's repetitive, boilerplate code that takes time and money.
2. The implementation must stay synchronized with the list of fields and automatic properties of the class. If you add a field to a struct, it's easy to forget to update both the <xref:System.Object.Equals%2A> and <xref:System.Object.GetHashCode> methods. This is an unnecessary source of human errors.

To avoid repetitive work and reduce maintenance errors, it's much better to implement the equality contract automatically during compilation.

## Why not Roslyn source generators?

To be fair, let's mention that you could use the Roslyn source generators API to implement the equality pattern because it doesn't require _modifying_ any hand-written members, only adding new ones. However, Roslyn source generators are low-level APIs and can require a lot of code to make them work.

In contrast, Metalama aspects are much easier to build. Metalama itself uses Roslyn source generators, but it adds several high-level features to dramatically improve your productivity:

- code templates,
- aspect inheritance,
- code validation (Roslyn generators cannot report errors, you need an additional analyzer),
- much simpler extensibility,
- and more.
