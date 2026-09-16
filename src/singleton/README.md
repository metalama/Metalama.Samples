---
uid: sample-singleton
summary: "The Singleton pattern ensures a class has only one instance, maintaining state consistency and reducing resource overhead. This series explores classic and modern Singleton implementations."
keywords: "singleton in C#, c# singleton, singleton implementation c#"
created-date: 2024-07-09
modified-date: 2024-09-09
---

# Implementing the Singleton pattern without boilerplate

The Singleton pattern is useful in scenarios where a class in a software system should have only one instance available to all components.

A single instance is what makes synchronized access to a shared resource work, for instance through locking. A Singleton is also useful when the object is expensive to build and one instance is enough for the whole application, saving both memory and processing.

The classic implementation of the Singleton pattern involves having a private constructor in the class, preventing it from being instantiated from outside and exposing a single instance through a static member.

With the advent of dependency injection and unit testing, the Singleton pattern has evolved. To allow several unit tests to be executed concurrently but in isolation from each other, we provide a new instance of each singleton to each unit test. Therefore, it is no longer true that a Singleton has a single instance per process but per context.

In this group of examples, we will explore how Metalama can automatically generate code and enforce the Singleton pattern, with or without dependency injection.

## In this series

We will explore various ways to improve the Singleton pattern:

| Article                                    | Description                                                                                                                        |
|--------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------|
| [Classic singleton](singleton-1/README.md) | We'll start with the classic Singleton pattern, where the class has a private constructor and a public static property `Instance`. |
| [Modern singleton](singleton-2/README.md)  | We'll discover how to improve safety for "singleton" classes that are used with dependency injection and testing.                  |



