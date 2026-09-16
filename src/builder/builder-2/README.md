---
uid: sample-builder-2
summary: ""
keywords: "builder pattern C#, c# builder pattern, builder design pattern C#"
created-date: 2024-09-30
modified-date: 2024-09-30
level: 300
---

# Builder example, step 2: Handling derived types

[!metalama-project-buttons .]

In the previous article, we assumed the type hierarchy was flat. Now, we will consider type inheritance, handling cases where the base type already has a builder.

Our objective is to generate code like in the following example, where the `WebArticle` class derives from `Article`. Notice how `WebArticle.Builder` derives from `Article.Builder` and how `WebArticle` constructors call the `base` constructors of `Article`.

[!metalama-compare ../../tests/Metalama.Samples.Builder2.Tests/DerivedType.cs]

## Step 1. Preparing to report errors

A general best practice when implementing patterns using an aspect is to consider the case where the pattern has been implemented _manually_ on the base type and to report errors when hand-written code does not adhere to the conventions we have set for the patterns. For instance, the previous article set some rules regarding the generation of constructors. In this article, the aspect will assume that the base types (both the base source type and the base builder type) define the expected constructors. Otherwise, we will report an error. It's always better for the user than throwing an exception.

Before reporting any error, we must declare a <xref:Metalama.Framework.Diagnostics.DiagnosticDefinition> static field for each type of error.

[!metalama-file BuilderDiagnosticDefinitions.cs]

For details, see <xref:diagnostics>.

## Step 2. Finding the base type and its members

We can now inspect the base type and look for artifacts we will need: the constructors, the `Builder` type, and the constructors of the `Builder` type. If we don't find them, we report an error and quit.

[!metalama-file GenerateBuilderAttribute.cs marker="FindBaseType"]

## Step 3. Creating the Builder type

Now that we have found the artifacts in the base type, we can update the rest of the <xref:Metalama.Framework.Aspects.IAspect`1.BuildAspect*> method to use them.

In the snippet that creates the `Builder` type, we specify the `Builder` of the base type as the base type of the new `Builder`:

[!metalama-file GenerateBuilderAttribute.cs marker="IntroduceBuilderType"]

Note that we set the `whenExist` parameter to `OverrideStrategy.New`. This means we will generate a `new` class if the base type already contains a `Builder` class.

## Step 4. Mapping properties

To discover properties, we now use the <xref:Metalama.Framework.Code.INamedType.AllProperties> collection which, unlike <xref:Metalama.Framework.Code.INamedType.Properties>, includes properties defined by base types. We added an `IsInherited` property into the `PropertyMapping` field.

Here is how we updated the code that discovers properties:

[!metalama-file GenerateBuilderAttribute.cs marker="CreatePropertyMap"]

The code that creates properties must be updated too. We don't have to create builder properties for properties of the base type since these properties should already be defined in the base builder type. If we don't find such a property, we report an error.

[!metalama-file GenerateBuilderAttribute.cs marker="CreateProperties"]

Note that we could do more validation, such as checking the property type and its visibility.

## Step 5. Updating constructors

All constructors must be updated to call the `base` constructor. Let's demonstrate the technique with the public constructor of the `Builder` class.

Here is the updated code:

[!metalama-file GenerateBuilderAttribute.cs marker="CreateBuilderConstructor"]

The first part of the logic is unchanged: we add a parameter for each required property, including inherited ones. Then, when we have a base class, we call the base constructor. First, we set the <xref:Metalama.Framework.Code.DeclarationBuilders.IConstructorBuilder.InitializerKind> of the new constructor to <xref:Metalama.Framework.Code.ConstructorInitializerKind.Base>. Then, for each parameter of the base constructor, we find the corresponding parameter in the new constructor, and we call the <xrefMMetalama.Framework.Code.DeclarationBuilders.IConstructorBuilder.AddInitializerArgument*> method to add an argument to the call to the `base()` constructor. If we don't find this parameter, we report an error.

## Step 6. Other changes

Other parts of the `BuildAspect` method and most templates must be updated to take inherited properties into account. Please refer to the source code of the example on GitHub for details (see the links at the top of this article).

## Conclusion

Handling type inheritance is generally not a trivial task because you have to consider the possibility that the base type does not define the expected declarations. Reporting errors is always better than failing with an exception, and certainly better than generating invalid code.

In the next article, we will see how to handle properties whose type is an immutable collection.
