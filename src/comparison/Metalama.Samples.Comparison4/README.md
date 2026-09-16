---
uid: sample-comparison-4
---

# Equality comparison example, step 4: Customizing equality comparers

In previous articles, we demonstrated how to [automatically implement an equality comparison pattern](xref:sample-comparison-1). We covered [handling type inheritance](xref:sample-comparison-2) and enhancing our API to allow users to [select specific equality members](xref:sample-comparison-3). However, our implementation had strict limitations: we always used the default comparer (<xref:System.Collections.Generic.EqualityComparer%601.Default?text=EqualityComparer&lt;T%gt;.Default>) and employed alphabetical ordering of members.

Let's dig deeper into the rabbit hole and add two new features to the aspect library:

- The ability to specify or optimize the order in which members are compared.
- The ability to use different equality comparers, such as <xref:System.StringComparer.OrdinalIgnoreCase?text=StringComparer.OrdinalIgnoreCase>.

Our goal is to support code constructs as illustrated below:

[!metalama-files Entity.cs Person.cs]

## Step 1. Designing member ordering

At first glance, it might seem that we can compare equality members in any order, as the result should be identical regardless. However, optimizing the order of comparison can lead to significant performance improvements.

Ideally, we want to first evaluate members that are _faster to compare_ and _more likely to differ_.

Consider an `EntityKey` type with two members: `string EntityName` and `int EntityId`, a typical data object design from the early 2000s. Since it's much cheaper to compare two `int` values than two `string` values, and since values are more likely to differ (with thousands of rows per table but dozens of tables), we will always want to compare `EntityId` first.

Our aspect could make part of this decision on its own: it's easy to hardcode that some comparisons are faster than others and prioritize those.

However, our aspect won't be able to determine the probability of equality. This means we also need the ability for the user to specify the order of evaluation.

Therefore, we chose the following design:

- The `EqualityMemberAttribute` class will have an `Order` property, which can be set manually. Its default value will be 1000.
- When two members have the same order, we order them by _execution cost_, based on hard-coded rules that specify, for instance, that it's cheaper to compare two `int` than two `string`. The cost will be returned by the `GetCost` method of the `EqualityMemberAttribute` class.

[!metalama-file EqualityMemberAttribute.cs marker="Ordering"]

As you can see, the `GetOrder` implementation can become arbitrarily complex, and we will not explore this further here.

## Step 2. Designing equality comparer customization

We want to allow users to specify their own <xref:System.Collections.Generic.IEqualityComparer%601> implementation. We chose a mechanism where the `EqualityMemberAttribute` class can be derived. We will provide, as examples, two implementations: `StringComparerAttribute`, which will allow the choice of <xref:System.StringComparison> mode, and `DateComparerAttribute`, which will compare the date component of a <xref:System.DateTime>, ignoring the time component.

For the `ImplementEquatable` aspect, the equality comparer for every member is just an `IExpression`. Therefore, we will define a `GetComparerExpression` virtual method on `EqualityMemberAttribute`:

[!metalama-file EqualityMemberAttribute.cs member="EqualityMemberAttribute.GetComparerExpression"]

Let's look at the implementation of `[StringEqualityMember]`:

[!metalama-file StringEqualityMemberAttribute.cs]

As you can see from the code:

- The main code is the `GetComparerExpression` override, which returns an expression representing the comparer (for instance, <xref:System.StringComparer.Ordinal>).
- `BuildEligibility` restricts this aspect to fields and properties of type `string`.
- If you want a custom, non-system comparer, you can provide your own as a public class with a public static property. Look at `TrimmingStringEqualityComparer` in the source code for details.

## Step 3. Updating BuildAspect

Now that we have the elements of our new API in place, we can update the aspect, starting from the information-gathering step in the <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> method.

In previous articles, all the templates needed to do their job was a list of <xref:Metalama.Framework.Code.IFieldOrProperty>. Now, for each member, we will also need a reference to the aspect instance, so we can call the `GetComparerExpression` method. Therefore, we define the compile-time tuple `EqualityMemberInfo` to represent this data:

[!metalama-file EqualityMemberInfo.cs]

We can now update the logic that collects the fields. We will obtain the `EqualityMemberAttribute` aspect instance and sort the aspects by explicit order and cost.

[!metalama-file ImplementEquatableAttribute.cs marker="GetFields"]

## Step 4. Updating the templates

The templates for `Equals(T)` and <xref:System.Object.GetHashCode> must be updated to take the comparer instance from the `GetComparerExpression` method.

First, we update the signatures to accept a list of `EqualityMemberInfo` instead of just <xref:Metalama.Framework.Code.IFieldOrProperty>.

Then, we can call the `GetComparerExpression` method to get the <xref:System.Collections.Generic.IEqualityComparer%601>. Here is how we do it in `Equals(T)`:

[!metalama-file ImplementEquatableAttribute.cs marker="CompareFields"]

There should be no surprises in this code!

## Conclusion

This article has opened the door to a whole new level of meta-programming, where you can define extensibility APIs for your aspects. We showed how to design aspect libraries by providing compile-time extension points, such as the <xref:System.Collections.Generic.IEqualityComparer%601>. Incidentally, this is something that's very difficult to achieve with pure Roslyn source generators. So, in this article, we've found another reason to use Metalama.

As you can imagine, there is much more you can do to customize the equality comparison aspect. We've just scratched the surface.

As an aspect developer, remember that your role is to maximize the productivity of your team, not to confuse them. It's your job to create well-designed, well-tested aspects, with complete error reporting, and not force other team members to engage in meta-programming unless they really want to. In this spirit, you should maintain a collection of equality member attributes covering all the needs of your team, ensuring they interact nicely and intuitively with each other.
