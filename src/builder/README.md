---
uid: sample-builder
summary: "This chapter explains how to automatically implement the Builder pattern in C# using Metalama."
keywords: "builder pattern C#, c# builder pattern, builder design pattern C#"
level: 300
created-date: 2024-09-30
modified-date: 2024-09-30
---

# Implementing the Builder pattern without boilerplate

The rise of _immutable_ objects has made the Builder pattern common in C#. Its most frequent use today is to simplify the creation of an immutable object through a mutable one, called a _builder_, that exists only to construct it. The <xref:System.Collections.Immutable> namespace is a good example: each collection type has its own mutable builder.

Without builders, constructing complex immutable objects can be cumbersome because all properties must be set at once. The Builder pattern allows you to split the object creation process into several steps.

Typically, a builder type would almost be a copy of the corresponding immutable type, with mutable properties instead of read-only ones. An exception to this rule is for properties of _collection_ type. Typically, the immutable type would have properties of other immutable types, while the builder will have properties of mutable types.

A proper implementation of the Builder pattern should exhibit the following features:

* A `Builder` constructor accepting all required properties.
* A writable property in the `Builder` type corresponding to each property of the build type. For properties returning an immutable collection, the property of the `Builder` type should be read-only but return the corresponding _mutable_ collection type.
* A `Builder.Build` method returning the built immutable object.
* The ability to call an optional `Validate` method when an object is built.
* In the source type, a `ToBuilder` method returning a `Builder` initialized with the current values.

To support these features, some implementation details are necessary, such as internal constructors for the source and the `Builder` type.

As you can imagine, implementing the Builder pattern by hand is a tedious and repetitive task. Fortunately, precisely because it is repetitive, it can be automated using a Metalama aspect.

## Alternative

An alternative to the Builder pattern in C# is the use of `record` types, `init` properties, and the `with` keyword. Using vanilla C# is _always_ better than using an aspect if it covers your needs.

However, using records has some limitations:

* If an object must be built in several steps, several instances of a `record` are required (modified in chain with the `with` keyword), while a single `Builder` instance would be necessary in this case.
* Handling collections is more convenient with the Builder pattern.
* It is not possible to validate a `record` before it is made accessible.


## In this series

In this series, we explain, step by step, how to implement the Builder pattern:

| Article | Description |
| ------- | ----------- |
| [Getting started](builder-1/README.md) | Presents a first working version of the GenerateBuilder aspect. |
| [Handling derived classes](builder-2/README.md) | Discusses how to handle non-sealed types. |
| [Handling immutable collections](builder-3/README.md) | Discusses how to generate mutable properties in the Builder class from immutable properties in the source class. |
