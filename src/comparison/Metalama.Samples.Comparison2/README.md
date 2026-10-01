---
uid: sample-comparison-2
---

# Equality comparison example, step 2: Support type inheritance

In the [previous step](xref:sample-comparison-1), we implemented a basic version of the equality comparison aspect, but we deliberately avoided addressing the scenario where a class with an equality contract has derived classes that can add members to the equality contract. In this article, we'll address this limitation.

We'll use the notation `T` to refer to the type being advised, and `TBase` for the closest ancestor type that implements the equality pattern. Note that `TBase` is not necessarily the immediate base class, because it's valid for an intermediate class not to add any equality member. In this case, we need to look further into the ancestors.

Let's outline the different changes we need to make to the aspect.

Regarding the `Equals` methods, here's how we need to modify the aspect:

- From the perspective of the _base_ class, we must make the `Equals(T)` method `virtual` so it can be overridden in a derived class.
- From the perspective of the _child_ class, we must:
  - Override the `Equals(TBase)` method so that it calls _our_ `Equals(T)`, ensuring our new equality members are taken into account.
  - Call `base.Equals(TBase)` from `Equals(T)`, so our method considers the base equality members.

For <xref:System.Object.GetHashCode>, we also need to call the base method to integrate the base members into the hash code. However, we should be careful _not_ to call the root <xref:System.Object.GetHashCode> because it returns a hash of the object's address.

## Step 1. Making the aspect inheritable

The first thing we need to do is mark our aspect as `[Inheritable]`. This allows the aspect to be applied to derived classes automatically when it's applied to a base class.

[!metalama-file ImplementEquatableAttribute.cs marker="Header"]

To learn more, see <xref:aspect-inheritance>.

## Step 2. Identifying the base methods

Remember that we decided, in the [previous article](xref:sample-comparison-1), to split the <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> method in two parts: first information gathering, then adding advice. We need to enhance the first part and identify artifacts from the base classes.

The first thing we need to do is identify the base `Equals(TBase)` method. We iterate through ancestor types and look for a method named `Equals` with a single parameter of the ancestor type. It uses the `IsAccessibleFrom` method to check that the method is accessible from the current type.

[!metalama-file ImplementEquatableAttribute.cs marker="FindBaseEquals"]

Note that this snippet calls `CheckMethodOverridable` to verify that the base `Equals` method can be overridden. If not, this method reports an error and does not implement the aspect. Let's look at the implementation of `CheckMethodOverridable`:

[!metalama-file ImplementEquatableAttribute.cs member="ImplementEquatableAttribute.CheckMethodOverridable"]

The `CheckMethodOverridable` method calls `builder.Diagnostics.Report` to report an error if the base method cannot be overridden. The error itself must be declared as a static field or property of a compile-time type. Reporting an error does not stop the execution of `BuildAspect`, so we need an explicit `return` statement. However, any advice provided by `BuildAspect` will be ignored if any error is reported.

[!metalama-file DiagnosticDefinitions.cs]

To learn more about reporting errors and warnings, see <xref:diagnostics>.

To identify the base `Equals` method, the easiest approach is to use the <xref:Metalama.Framework.Code.INamedType.AllMethods> collections of the base type, which includes all methods of the base type, including all inherited methods.

We did not use this approach to find the base `Equals` method because it would have been more difficult: filtering <xref:Metalama.Framework.Code.INamedType.AllMethods> for methods whose only parameter is of the same type as the declaring type, would potentially return several methods, one for each ancestor, and we would still need to choose the _closest_ ancestor.

[!metalama-file ImplementEquatableAttribute.cs marker="FindBaseGetHashCode"]

## Step 3. Updating the Equals(T) method

Now that we've identified the base methods, we can update the code that introduces the members. Let's start with the strongly-typed `Equals(T)` method, where `T` is the _current_ type.

The code snippet in the <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> method is updated to the following:

[!metalama-file ImplementEquatableAttribute.cs marker="IntroduceTypedEquals"]

Note two changes in this snippet:

1. We are passing `baseEqualsMethod` to the template.
2. We are marking the method as `virtual`, unless the type is sealed.

To the `TypedEqualsTemplate` method, we add the <xref:Metalama.Framework.Code.IMethod> `baseEqualsMethod` parameter, plus the following snippet:

[!metalama-file ImplementEquatableAttribute.cs marker="TypedEqualsTemplate_CallBaseMethod"]

Note the use of `With(InvokerOptions.Base)`. By default, invoking a method (using <xref:Metalama.Framework.Code.Invokers.IMethodInvoker.Invoke%2A>) results in calling the final override (i.e., generating `this.Equals`) of this method, even if the method is in a base type. You must use the <xref:Metalama.Framework.Code.Invokers.InvokerOptions.Base> option so that Metalama generates `base.Equals`.

## Step 4. Overriding the Equals(TBase) method

We must now override the `Equals` method of the base type, if any.

We add the following snippet to the <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> method:

[!metalama-file ImplementEquatableAttribute.cs marker="IntroduceBaseTypeEquals"]

Here is the template for the `Equals(TBase)` override:

[!metalama-file ImplementEquatableAttribute.cs member="ImplementEquatableAttribute.BaseTypeEqualsTemplate"]

## Step 5. Updating the GetHashCode method

The last step is to update the `GetHashCode` method to make it call `baseGetHashCodeMethod`, which we previously identified.

This requires the following changes:

1. In `BuildAspect`, pass `baseGetHashCodeMethod` to <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceMethod%2A>.
2. Add an <xref:Metalama.Framework.Code.IMethod>? `baseGetHashCodeMethod` parameter to the template for the <xref:System.Object.GetHashCode> method.
3. Add the following snippet to the template:

[!metalama-file ImplementEquatableAttribute.cs marker="CallBaseGetHashCode"]

## Result

Our aspect can now properly handle type inheritance.

To demonstrate this, let's extract the `Entity` class as a base for the `Person` class. You can check that the equality pattern now properly takes class inheritance into account.

[!metalama-files Entity.cs Person.cs EntityKey.cs]

In the [next article](xref:sample-comparison-3), we will make it possible to add individual fields or properties to the equality contract.

> [!div class="see-also"]
> <xref:aspect-inheritance>
> <xref:diagnostics>
