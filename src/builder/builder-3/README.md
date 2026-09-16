---
uid: sample-builder-3
summary: ""
keywords: "builder pattern C#, c# builder pattern, builder design pattern C#"
created-date: 2024-09-30
modified-date: 2024-09-30
level: 400
---

# Builder example, step 3: Handling immutable collection properties

[!metalama-project-buttons .]

In the previous articles, we created an aspect that implements the `Builder` pattern for properties of "plain" types. However, properties of collection types require different handling.

Since the Builder pattern is typically used to build immutable objects, it is good practice for properties of the immutable class to be of an immutable type, such as <xref:System.Collections.Immutable.ImmutableArray`1> or <xref:System.Collections.Immutable.ImmutableDictionary`2>. In the `Builder` class, though, it's more convenient if the collections are mutable. For instance, for a source property of type `ImmutableArray<string>`, the builder property could be an `ImmutableArray<string>.Builder`.

In this article, we'll update the aspect so that the collection properties of the `Builder` class are of the _builder_ collection type.

Additionally, we want the collection properties in the `Builder` type to be _lazy_, meaning we only allocate a collection builder if the collection is evaluated.

Here is an example of a transformation performed by the aspect.

[!metalama-compare ../../tests/Metalama.Samples.Builder3.Tests/ImmutableArray.cs]

## Step 1. Setting up more abstractions

We'll now update the aspect to support two kinds of properties: standard ones and properties of an immutable collection type. We'll only support collection types from the <xref:System.Collections.Immutable> namespace, but the same approach can be used for different types.

Since we have two kinds of properties, we'll make the `PropertyMapping` class _abstract_. It will have two implementations: `StandardPropertyMapping` and `ImmutableCollectionPropertyMapping`. Any implementation-specific method must be abstracted in the `PropertyMapping` class and implemented separately in derived classes.

These implementation-specific methods are as follows:

* `GetBuilderPropertyValue() : IExpression` returns an expression that contains the value of the `Builder` property. The type of the expression must be the type of the property in the _source_ type, not in the builder type. For standard properties, this will return the builder property itself. For immutable collection properties, this will be the new immutable collection constructed from the immutable collection builder.
* `ImplementBuilderArtifacts()` will be called for non-inherited properties and must add declarations required to implement the property. For standard properties, this is just the public property in the `Builder` type. For immutable collections, this is more complex and will be discussed later.
* `TryImportBuilderArtifactsFromBaseType` will be called for inherited properties and must find the required declarations from the base type.
* `SetBuilderPropertyValue` is the _template_ used in the copy constructor to store the initial value of the property.

The `BuilderProperty` property of the `PropertyMapping` class now becomes an implementation detail and is removed from the abstract class.

Here is the new `PropertyMapping` class:

[!metalama-file PropertyMapping.cs]

Note that the `PropertyMapping` class now implements the (empty) <xref:Metalama.Framework.Aspects.ITemplateProvider> interface. This is required because `SetBuilderPropertyValue` is an _auxiliary template_, i.e. a template called from another top-level template. Note also that `SetBuilderPropertyValue` cannot be abstract due to current limitations in Metalama, so we had to make it virtual. For details regarding auxiliary templates, see <xref:auxiliary-templates>.

The implementation of `PropertyMapping` for standard properties is directly extracted from the aspect implementation in the previous article.

[!metalama-file StandardPropertyMapping.cs]

The `TryFindBuilderPropertyInBaseType` helper method is defined here:

[!metalama-file PropertyMapping.Helpers.cs member="PropertyMapping.TryFindBuilderPropertyInBaseType"]

## Step 2. Updating the aspect

Both the `BuildAspect` method and the templates must call the abstract methods and templates of `PropertyMapping`.

Let's look, for instance, at the code that used to create the builder properties in the `Builder` nested type. You can see how the implementation-specific logic was moved to `PropertyMapping.ImplementBuilderArtifacts` and `PropertyMapping.TryImportBuilderArtifactsFromBaseType`:

[!metalama-file GenerateBuilderAttribute.cs marker="CreateBuilderProperties"]

The aspect has been updated in several other locations. For details, please refer to the source code by following the link to GitHub at the top of this article.

## Step 3. Adding the logic specific to immutable collections

At this point, we can run the same unit tests as for the previous article, and they should execute without any differences.

Let's now focus on implementing support for properties whose type is an immutable collection.

As always, we should first design a pattern at a conceptual level, and then switch to its implementation.

To make things more complex with immutable collections, we must address the requirement that collection builders should not be allocated until the user evaluates the public property of the `Builder` type. When this property is required, we must create a collection builder from the initial collection value, which can be either empty or, if the `ToBuilder()` method was used, the current value in the source object.

Each property will be implemented by four artifacts:
- A field containing the property _initial value_, which can be either empty or, when initialized from the copy constructor, set to the value in the source object.
- A nullable field containing the _collection builder_.
- The public property representing the collection, which lazily instantiates the collection builder.
- A method returning the immutable collection from the collection builder if it has been defined, or returning the initial value if undefined (which means that there can be no change).

These artifacts are built by the `ImplementBuilderArtifacts` method of the `ImmutableCollectionPropertyMapping` class and then used in other methods and templates.

[!metalama-file ImmutableCollectionPropertyMapping.cs]

## Conclusion

Handling different kinds of properties led us to use more abstraction in our aspect. As you can see, meta-programming, like other forms of programming, requires a strict definition of concepts and the right level of abstraction.

Our aspect now correctly handles not only derived types but also immutable collections.

