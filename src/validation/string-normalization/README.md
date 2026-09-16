---
uid: sample-string-normalization
level: 100
summary: "This project demonstrates simple string normalization aspects like trimming, upper-casing, and UTF normalization, using a base aspect class to handle common behaviors."
keywords: "string normalization, trimming, upper-casing, UTF normalization, .NET"
created-date: 2024-09-05
modified-date: 2024-09-09
---

# Automatically trimming, upper-casing or normalizing strings

[!metalama-project-buttons .]

This project contains very simple aspects that implement several cases of string normalization:

* Trimming,
* Upper-casing,
* UTF normalization.

They transform the code as follows:

[!metalama-compare Class1.cs ]

## Base aspect class

We created a base class that shares the common behaviors for all aspects:

[!metalama-file StringContractAspect.cs ]

This class derives from <xref:Metalama.Framework.Aspects.ContractAspect> because this base class conveniently supports fields, properties and parameters at the same type. As its name does not suggest, this class can be used to modify the value, not just to validate it. 

The `StringContractAspect` class overrides <xref:Metalama.Framework.Eligibility.IEligible`1.BuildEligibility*> to limit the eligibility of the aspects to fields, properties and parameters of type `string`. For details regarding eligibility, see <xref:eligibility>.

It also defines a helper method to determine whether the field, property or parameter is nullable.

## Concrete aspect classes

With this base class, the concrete implementations are almost trivial. The only difficulty is that the nullable and non-nullable cases must be handled separately.

Let's look at the implementation of the Trim aspect:

[!metalama-file TrimAttribute.cs ]




