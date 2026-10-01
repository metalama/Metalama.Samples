---
uid: sample-comparison-1
---

# Equality comparison example, step 1: a minimal implementation

This example builds the simplest possible aspect to automatically implement the equality comparison pattern. We'll call it `[ImplementEquatable]` because it implements the <xref:System.IEquatable%601> interface.

Of course, just implementing the <xref:System.IEquatable%601> interface isn't enough for a complete equality comparison pattern. For a type `T`, a full equality comparison pattern involves the following operations:

1. Add the <xref:System.IEquatable%601> interface to the class.
2. Add a method `public bool Equals(T? other)` that compares the current object with another one, field by field.
3. Override the <xref:System.Object.Equals(System.Object)> method.
4. Override the <xref:System.Object.GetHashCode> method.
5. Add the `==` and `!=` operators to the class.

Once this aspect is complete, it will be capable of generating code like this:

[!metalama-files Person.cs EntityKey.cs]

To keep things simple, we'll ignore type inheritance in this article.

## Step 1. Create the ImplementEquatableAttribute class

To start off, we'll create the `ImplementEquatableAttribute` class and have it derive from Metalama's <xref:Metalama.Framework.Aspects.TypeAspect> class, making it both a custom attribute and a type-level Metalama aspect.

The entry point of any Metalama aspect is the <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> method, so that's where we begin. Remember, this method must perform all operations to implement the pattern as listed above.

[!metalama-file ImplementEquatableAttribute.cs marker="ImplementEquatableAttribute"]

## Step 2. Identifying the equatable members

When authoring aspects, it's good practice to split the <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> implementation into two parts: data gathering and analysis (i.e., the code model), followed by adding transformations (advice) to the compilation pipeline.

So first, we'll identify the members that need to be considered in the equality comparison. This includes all instance fields and automatic properties, except those implicitly defined by the compiler (like fields for automatic properties, hidden from C# code but visible in the Metalama code model).

An aspect should be fully deterministic, generating identical outputs from identical inputs in any situation. Therefore, we must order the collection of fields and automatic properties. For simplicity, we'll order by member name, but we'll revisit this in <xref:sample-comparison-4>.

Let's add the following code to the <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> method:

[!metalama-file ImplementEquatableAttribute.cs marker="IdentifyFields"]

## Step 3. Adding the IEquatable interface

Let's start adding advice (i.e., code transformations) to the type.

Our first piece of advice is to add the <xref:System.IEquatable%601> interface to the target type, where the generic parameter `T` is replaced with the target type itself. For this, we use the <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*?text=builder.ImplementInterface> method.

This is done with the following code:

[!metalama-file ImplementEquatableAttribute.cs marker="ImplementInterface"]

Note that this doesn't _implement_ the interface members. We'll do that next by adding the public `Equals` method.

To learn more about implementing interfaces, see <xref:implementing-interfaces>.

## Step 4. Adding the Equals(T) method

Now, we want to introduce the strongly-typed `Equals(T)` method, where `T` is the target type of our aspect. We'll have two code snippets for this: a template method and a call to <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceMethod%2A> in `BuildAspects`.

First, we define a _template_ method. Here's its definition:

[!metalama-file ImplementEquatableAttribute.cs member="ImplementEquatableAttribute.TypedEqualsTemplate"]

Notice the <xref:Metalama.Framework.Aspects.TemplateAttribute> custom attribute on the method. As the name suggests, it instructs Metalama to treat the method as a _template_ that can include both compile-time and run-time code. The compile-time code is executed at compile time, generating the code that will run at run time.

This method has three parameters:
- A compile-time type parameter `T` representing the target type of the aspect. This parameter only exists at compile time and won't appear in the generated code.
- A run-time parameter `other` that's part of the <xref:System.IEquatable%601.Equals%2A?text=IEquatable&lt;T&gt;.Equals(T)> method signature.
- A compile-time parameter `fields` representing the list of fields and automatic properties to be compared.

The rest of the implementation is straightforward and documented with inline comments.

Note that we're relying on the default <xref:System.Collections.Generic.EqualityComparer%601> for each field type. In the [fourth article](xref:sample-comparison-4), we'll explore how to customize this logic.

This template doesn't automatically add itself to the target type. We must add the following code to the <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> method:

[!metalama-file ImplementEquatableAttribute.cs member="ImplementEquatableAttribute.TypedEqualsTemplate"]

We call the <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceMethod%2A> method from our <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> method:

[!metalama-file ImplementEquatableAttribute.cs marker="IntroduceTypedEquals"]

Let's look at the arguments we pass to this method:

1. The first argument is the name of the _template method_ we defined above.
2. `args` are the arguments we're binding to `T` and `fields`, the compile-time parameters (both type and normal parameters) of the template method.

By definition, the run-time parameter of the template method must not (and cannot) be bound at compile time.

You can learn more about introducing methods in <xref:introducing-members>, and about template parameters in <xref:template-parameters>.

## Step 5. Overriding the default Equals(object) method

Next, let's override the default <xref:System.Object.Equals%2A> method, so facilities that don't support the <xref:System.IEquatable%601> interface use the correct equality implementation.

As with any member introduction, two steps are involved: implementing the template and calling <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceMethod%2A> from <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A>.

The template should be straightforward:

[!metalama-file ImplementEquatableAttribute.cs member="ImplementEquatableAttribute.UntypedEqualsTemplate"]

Note that we're using `meta.This`, which compiles into `this`, i.e., the current instance at run time. By contrast, `this` in the template refers to the compile-time aspect instance.

Here's the code snippet in the <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> method:

[!metalama-file ImplementEquatableAttribute.cs marker="IntroduceUntypedEquals"]

The `whenExists` parameter determines the strategy if the member already exists in the target type or an ancestor type. We use <xref:Metalama.Framework.Aspects.OverrideStrategy> to specify that we want to override the member in this case.

## Step 6. Overriding the GetHashCode method

To implement the <xref:System.Object.GetHashCode> method, we chose to rely on the <xref:System.HashCode> system class, which combines different values into a single hash.

The rest of the implementation follows the principles we've already explained.

Here's the template:

[!metalama-file ImplementEquatableAttribute.cs member="ImplementEquatableAttribute.GetHashCodeTemplate"]

And here's the snippet to add to <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A>:

[!metalama-file ImplementEquatableAttribute.cs marker="IntroduceGetHashCode"]

## Step 7. Adding the operators

The finishing touch, and a best practice, is to introduce the `==` and `!=` operators. This can be done by calling the <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceBinaryOperator%2A> method from <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A>.

Let's first define the templates:

[!metalama-file ImplementEquatableAttribute.cs marker="OperatorTemplates"]

We can now advise the type:

[!metalama-file ImplementEquatableAttribute.cs marker="IntroduceOperators"]

## Summary

Implementing an equality contract involves a lot of boilerplate code, but fortunately, most of it can be automatically generated by an aspect.

In this article, we've shown how to implement all the components required by this pattern: the <xref:System.IEquatable%601.Equals%2A?text=Equals(T)>, <xref:System.Object.Equals(System.Object)?text=Equals(object)>, <xref:System.Object.GetHashCode>, and the `==` and `!=` operators.

We used compile-time template parameters (including type parameters) to pass data from the <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> method to the templates. We generated slightly different code for value and reference types using compile-time conditions.

However, we've limited ourselves to very simple cases. In the [next article](xref:sample-comparison-2), we'll explore how to handle type inheritance in reference types.

In the <xref:sample-comparison-2>, we will add support for type inheritance.

> [!div class="see-also"]
> <xref:implementing-interfaces>
> <xref:introducing-members>
> <xref:template-parameters>
