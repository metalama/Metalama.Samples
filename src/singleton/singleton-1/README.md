---
uid: sample-singleton-1
summary: "This document shows how to implement the classic Singleton pattern using an aspect. This pattern ensures a single instance of a class with a private constructor and a static instance property."
keywords: "Singleton pattern"
created-date: 2024-06-23
modified-date: 2024-09-09
---

# Example: The Classic Singleton Pattern

[!metalama-project-buttons .]

The Singleton pattern requires that a single instance of a class exists throughout the application's lifetime.

In its classic version, the Singleton pattern in C# involves the following elements:

* A private constructor, or more precisely, the absence of any public constructor, which ensures that the class cannot be externally instantiated.
* A static read-only property or a field-backed method exposing the unique instance of the class.

A performance counter manager serves as an ideal example for the Singleton pattern, as its metrics must be consistently gathered across the entire application.

[!metalama-file ../singleton-0/PerformanceCounterManager.cs]

If you frequently use the Singleton pattern, you might start noticing several issues with this code:

1. **Clarity**: It's not immediately clear that the type is a Singleton. You need to parse more code to understand the pattern the class follows.
2. **Consistency**: Different team members may implement the Singleton pattern in slightly different ways, making the codebase harder to learn and understand. For instance, the `Instance` property could have a different name, or it could be a method instead.
3. **Boilerplate**: Each Singleton class repeats the same code, which is tedious and could potentially lead to bugs due to inattention.
4. **Safety**: There's nothing preventing someone from making the constructor public and then creating multiple instances of the class. You would typically rely on code reviews to detect violations of the pattern.

## Step 1: Generating the Instance property on the fly

To ensure consistency and avoid boilerplate code, we'll add code to `SingletonAttribute`. This will implement the repetitive part of the Singleton pattern for us by introducing the `Instance` property (see <xref:introducing-members>), with an initializer that invokes the constructor.

First, we add a template property to the aspect class, which outlines the shape of the `Instance` property (we can't reference the type of the Singleton here, so we use `object` as the property type instead and replace it later):

[!metalama-file SingletonAttribute.cs marker="InstanceTemplate"]

Then, we add code to the `BuildAspect` method to actually introduce the `Instance` property:

[!metalama-file SingletonAttribute.cs marker="IntroduceInstanceProperty"]

Here, we call <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceProperty*>, specifying the type into which the property should be introduced, the name of the template, and a lambda that is used to customize the property further. Inside the lambda, we replace the `object` type with the actual type of the Singleton class and set the initializer to invoke the constructor. We use <xref:Metalama.Framework.Code.SyntaxBuilders.ExpressionBuilder> to build the expression that calls the constructor, including the <xref:Metalama.Framework.Code.SyntaxBuilders.SyntaxBuilder.AppendTypeName*> method, which ensures that the type name is correctly formatted.

The resulting Singleton class is a bit simpler, and doing this automatically ensures that all Singletons in the codebase are implemented in the same way:

[!metalama-compare PerformanceCounterManager.cs]

## Step 2: Verifying that constructors are private

To ensure safety, we can verify that all constructors are private and produce a warning (see <xref:diagnostics>) if they are not. To do this, we first add a definition of the warning as a `static` field to the aspect class:

[!metalama-file SingletonAttribute.cs marker="PrivateConstructorDiagnostic"]

The type of the field is <xref:Metalama.Framework.Diagnostics.DiagnosticDefinition`1>, where the type argument specifies the types of parameters used in the diagnostic message as a tuple. The message uses the same format string syntax as the <xref:System.String.Format*?displayProperty=nameWithType> method.

We then add code to the `BuildAspect` method to check if the constructor is private and produce a warning if it isn't:

[!metalama-file SingletonAttribute.cs marker="PrivateConstructorReport"]

To do this, we iterate over all constructors of the type, check the <xref:Metalama.Framework.Code.IMemberOrNamedType.Accessibility> for each of them, and then report the warning specified above if the accessibility is not <xref:Metalama.Framework.Code.Accessibility.Private>. Note that we are skipping the implicitly defined default constructor because this case will be covered in step 3. We specify the formatting arguments for the diagnostic message as a tuple using the <xref:Metalama.Framework.Diagnostics.DiagnosticDefinition`1.WithArguments*> method. We also explicitly set the location of the diagnostic to the constructor; otherwise, the warning would be reported by default at the type level, because we're reporting it through the <xref:Metalama.Framework.Aspects.IAspectBuilder`1> for the Singleton type.

Notice the warning on the public constructor in the following code.

[!metalama-file PublicConstructorSingleton.cs]

## Step 3. Adding a private constructor

If the target class does not contain any user-defined constructor, the C# language implicitly defines a default public constructor. When the type is a Singleton, we want to introduce a _private_ default constructor instead.

[!metalama-file SingletonAttribute.cs marker="AddPrivateConstructor"]

## Aspect implementation

The complete aspect implementation is provided below:

[!metalama-file SingletonAttribute.cs]



