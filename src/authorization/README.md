---
uid: sample-authorization
level: 300
summary: "This example enforces a role-based authorization model with Metalama aspects: permission checks on methods, properties, fields and parameters, default permissions inherited by all business objects, and build-time configuration with hierarchical options."
keywords: "authorization, permissions, role-based access control, RBAC, security, aspect, hierarchical options, fabrics, contracts"
created-date: 2026-10-15
modified-date: 2026-10-16
---

# Enforcing authorization with aspects

[!metalama-project-buttons Metalama.Samples.Authorization.Demo]

This example shows how to enforce a non-trivial authorization model in business objects without writing permission checks by hand. Aspects add the checks to methods, properties, fields and parameters. A run-time policy decides whether the current user has the required permission on the object being accessed.

> [!WARNING]
> This example is a proof of concept. It is not tested for production use. See [Limitations](#limitations).

## Two layers: an abstract framework and a role-based model

The framework has two layers.

### Layer 1: the abstract authorization layer

The first layer is abstract. It does not assume any security model. It defines five concepts:

| Concept | Type | Description |
|---|---|---|
| Subject | `ISubject` | The user or service on whose behalf the code runs. |
| Permission | `Permission` | Something that the subject must have on an object, for instance `Read`, `Write` or `Approve`. Permissions are compared by name. |
| Securable | `ISecurable` | The object on which the permission is required. For a method or property, it is the current instance. For a parameter, it is the argument. Every class that implements `ISecurable` is secured and validated. |
| Policy | `IAuthorizationPolicy` | Decides whether a subject has a permission on a securable. |
| Security context | `ISecurityContext` | The current subject, the policy, and an optional auditor. `SecurityContext.Current` exposes it. |

The aspects, the options and the run-time entry point `AccessGuard` belong to this layer. They only ask one question: does the current subject have this permission on this object? The answer comes from the `IAuthorizationPolicy.IsGranted` method, and the abstract layer does not know how the policy computes it.

### Layer 2: the role-based model

The second layer is one implementation of `IAuthorizationPolicy`: the classic role-based access control model, in the `RoleBased` namespace.

* Roles are assigned to subjects on an object, for instance "Mia is department manager of Sales".
* Each object has a security parent and inherits the role assignments of its ancestors. For instance, an invoice inherits the role assignments of its department, and the department inherits those of the company.
* Rules allow or deny a permission to a role on a type of object, for instance "department managers are allowed to approve invoices". A rule on a base type also applies to derived types.
* A permission is granted when at least one rule allows it and no rule denies it.
* Computed roles are never assigned: the object computes them from its own data. Two are built in: every subject has `Everyone`, and the `Owner` of an object has `Owner` on this object only. Applications can add their own, such as the creator of an invoice.

### Replacing the role-based model

Other security models can replace the role-based layer without any change to the aspects or to the annotated business code. Only the policy and the securable objects change. For instance:

* A group-based model, where permissions are granted to groups of users, and a user has the permissions of all the groups they belong to.
* An access-control-list model, where each object stores the permissions granted directly to users, without an intermediate role.
* An attribute-based model, where the policy compares properties of the subject and of the object, for instance the department of the user and the department of the invoice.
* A composite policy, which delegates to several policies and combines their answers.

## Project structure

The security framework is separate from the application that uses it:

| Project | Content |
|---|---|
| `Metalama.Samples.Authorization` | The framework: the abstract layer (aspects, options, `AccessGuard`, security context) and the role-based layer. It does not know the invoicing domain. |
| `Metalama.Samples.Authorization.Demo` | An invoicing application that uses the framework: domain objects stored with Entity Framework Core, application services registered in a dependency injection container, and a console program that runs scenarios. |
| `tests/Metalama.Samples.Authorization.Tests` | Aspect tests: snapshots of the code that the aspects generate. |
| `tests/Metalama.Samples.Authorization.UnitTests` | Unit tests of the framework, on small test types. They do not depend on the demo. |
| `tests/Metalama.Samples.Authorization.Demo.UnitTests` | Unit tests of the security model of the demo application. |

## The aspects

### RequiresPermission

The `[RequiresPermission]` aspect adds a permission check before a method executes, or before a field or property is read or written. Applied to a parameter, it checks the permission on the argument instead of on the current instance.

Here is the invoice class of the demo, and the code that the aspects generate:

[!metalama-compare Metalama.Samples.Authorization.Demo/Sales/Invoice.cs]

The aspect implements `IAspect<IMethod>`, `IAspect<IFieldOrProperty>` and `IAspect<IParameter>`, so a single custom attribute can target the three kinds of declarations. Each `BuildAspect` method resolves the permission names and passes them to a template as compile-time arguments. The generated code calls `AccessGuard.Demand` with constant strings. There is no reflection and no allocation of permission objects at initialization.

[!metalama-file Metalama.Samples.Authorization/Framework/RequiresPermissionAttribute.cs]

A few details are worth noting:

* For a parameter, the aspect adds a contract with `AddContract`. Only parameters that have the attribute get a check.
* Every setter is checked, including a private one: the members of the class need the permission to change the state of the object. A property that only some operations change declares the permission of these operations as its write permission. For instance, `Customer.Department` requires `ChangeOwnership`, the permission of `Customer.MoveTo`.
* The getter of a collection property sets the owner of the `SecurableList<T>` that the property returns (see [Collections](#collections)).
* The aspect is `[Inheritable]`. An override of a secured virtual member gets the same check, so a derived class cannot skip it by not calling the base implementation. On an abstract member, the aspect secures all the overrides.
* The eligibility rules reject static members, members of a type that is not securable, `out` parameters, and parameters whose type is not securable. A method or property check is made on the current instance, so its type must implement `ISecurable`, and a static member has no instance at all. A parameter check needs a securable argument: passing an object that no policy can secure would always be denied, so it is a build error. Abstract members are rejected except through inheritance, with `builder.ExceptForInheritance().MustNotBeAbstract()`.
* When the attribute does not name a permission, the defaults of `AuthorizationOptions` apply. A method requires `Write` by default.

### Permissions are not transitive

The permission of a member is checked when the member is entered, and only then. It does not grant the permissions of the members that the member accesses: when a method reads or writes a secured field or property, of the same object or of another one, this access is checked on its own, for the current subject. For instance, a method that requires `Publish` and changes the `Body` property of the same object also requires `Read` and `Write` on the object, because it reads and writes `Body`.

This rule follows from the design: each check protects one member, whatever the code that calls it. A member that granted more rights to the code that it runs would turn every bug in its body into a way around the policy.

The rule has a consequence for aggregates. It is common that a user may see an aggregate, such as a total or a count, but not the details that it is computed from. An aggregate computed from secured members requires the permissions of these members. Private state is not an exception: every field and every property is secured, whatever its accessibility, so moving the details to a private field does not remove their check. The unit tests in `NestedAccessTests` verify both cases. The next aspect covers aggregates explicitly.

### GrantsPermissions

The `[GrantsPermissions]` aspect grants permissions while a member runs. On a method or a property, it grants them on the current instance. On a parameter, it grants them on the argument. In the demo, a department assistant may see whether the budget of their department is available, low or exhausted, but not its amounts. The `Status` property requires `Read`, and computes the level from `Amount` and `Remaining`, which require `ReadSensitive`:

```csharp
[RequiresPermission( PermissionNames.Read )]
[GrantsPermissions( DemoPermissions.ReadSensitive )]
public BudgetStatus Status
    => this.Remaining <= 0 ? BudgetStatus.Exhausted
        : this.Remaining < this.Amount / 10 ? BudgetStatus.Low
        : BudgetStatus.Available;
```

The generated code checks the permission of the member, then opens the grant:

[!metalama-test ../tests/Metalama.Samples.Authorization.Tests/GrantsPermissions.cs]

On a parameter, the grant gives the method access to an object that it receives. In the demo, `IInvoiceSender.SendAsync` requires `Send` on the invoice that it receives, so that no code can send an invoice for a user who may not send it, even without calling `Invoice.SendAsync`. Sending an invoice includes reading it, so the parameter also grants `Read` on the invoice. The two aspects are on the parameter of an interface method, and both are inherited by the parameter of every implementation, such as `OutboxInvoiceSender.SendAsync`:

```csharp
Task SendAsync(
    [RequiresPermission( DemoPermissions.Send )] [GrantsPermissions( PermissionNames.Read )] Invoice invoice,
    CancellationToken cancellationToken );
```

[!metalama-test ../tests/Metalama.Samples.Authorization.Tests/GrantsPermissions_Interface.cs]

`Department.CreateInvoice` requires `CreateInvoice` on the department, and increments the invoice counter of the department. The counter declares `CreateInvoice` as its write permission, which the method has already checked. The method grants `Read` on the department, so the numbering of the invoices does not depend on a rule of the policy. An earlier version of the demo opened a system scope with `SecurityContext.RunAsSystem` in `CreateInvoice`. A system scope disables every check, on every object, including the checks of the code that the method calls. The grant gives only the permission that the code needs, and only on the department. The constructor of the new invoice needs no grant (see [Constructing an object](#constructing-an-object)).

A method can also grant a permission that no role has. The status, the payments and the void reason of an invoice require `ChangeStatus` to change, and no rule of the policy gives `ChangeStatus` to a role. The methods of the workflow, `Approve`, `SendAsync`, `RecordPayment` and `Void`, grant it while they run, after their own permission has been checked. Changing the status is then a consequence of an operation of the workflow, and no other code can do it, even with the `Write` permission:

```csharp
[RequiresPermission( PermissionNames.Read, DemoPermissions.ChangeStatus )]
public InvoiceStatus Status { get; private set; }

[RequiresPermission( DemoPermissions.Approve )]
[GrantsPermissions( DemoPermissions.ChangeStatus )]
public void Approve()
```

The grant is a scope of the ambient security context. `SecurityContext.Grant` starts a new execution context, whose grants are an immutable dictionary (`ImmutableDictionary<ISecurable, ImmutableList<Permission>>`) that contains the outer grants and the new one. Disposing the scope restores the previous execution context. `AccessGuard.Demand` looks for a grant of the permission on the same object before it calls the policy. A grant also starts a new cache of decisions (see [Caching the decisions](#caching-the-decisions)). The grants are not members of `ISecurityContext`: an application implements this interface to provide the subject and the policy, and it should not have to implement grants.

The aspect is designed to give the smallest possible grant:

* The order of the aspects is critical. `[RequiresPermission]` is the outer aspect and `[GrantsPermissions]` is the inner one. The grant starts only after the permission of the member has been checked, so the grant cannot satisfy this permission. The parameter checks also run before the grant. The order is declared with `[assembly: AspectOrder]`. The same declaration orders `[SecurableType]`, which must be processed before `[RequiresPermission]` because it adds `[RequiresPermission]` aspects.
* The member must have a permission check, explicit or added by default. Otherwise any caller would get the granted permissions, so the validator reports the error `AUTH004`.
* The grant applies to one object only: the current instance, or the argument of the parameter. A member that reads the details of another object needs the permissions of the subject on that object. A null argument gets no grant.
* The grant applies only in the current context. When code switches to another subject with `SecurityContext.Use`, the grants of the previous subject do not apply.
* The grant ends when the member returns. In an `async` member, it covers the continuations. An iterator is not eligible, because its body runs when the caller enumerates the result, after the member has returned. An automatic property is not eligible either, because it has no code that could use the grant.
* The aspect is inherited, like `[RequiresPermission]`. An override or an implementation keeps the grant of the base member, so a derived class has the same behavior as its base class. On an abstract or interface member, the aspect is only inherited. A reviewer must therefore read the overrides of a member that grants permissions, and not only the member itself.

A grant takes precedence over the policy, including its denials. This is the purpose of the aspect, and also its risk: the body of the member must not return the details that the grant makes readable. Like an explicit exclusion from default securing, each grant is visible in the code and in a review. A grant also applies to the tasks that the member starts, because `AsyncLocal<T>` values flow into them. A member with a grant should therefore not start a task that runs after the member returns.

[!metalama-file Metalama.Samples.Authorization/Framework/GrantsPermissionsAttribute.cs]

### Constructing an object

The code that creates an object has unlimited rights on this object while it is initialized: from the first constructor that runs to the end of the object initializer. No other code can access the object before its construction ends, so setting it up is not an access to the data of somebody else. In the demo, the constructor of `Invoice` sets the department, the customer, the due date and the owner of the new invoice, and the creator needs neither `Write` nor `ChangeOwnership`:

```csharp
internal Invoice( Department department, Customer customer, string number, User? creator )
{
    this.Department = department;
    this.Customer = customer;
    this.Number = number;
    this.CreatorUser = creator;
    this.Owner = creator;
    this.DueDate = DateOnly.FromDateTime( DateTime.Today ).AddDays( 30 );
}
```

An object goes through three states:

1. While its constructors run, the code that creates the object, in its execution context, accesses the object without checks.
2. After the last constructor, and until the end of the object initializer, the creator can only set the properties of the object, as an object initializer does. Its getters and its methods throw an `InvalidOperationException`.
3. After the object initializer, the object is initialized and checked like any other object.

Any access from another execution context throws an `InvalidOperationException` until the object is initialized: an object that is not completely set up cannot be used by anybody else.

`[SecurableType]` adds three initializers to every securable class, one for each transition:

* A `BeforeInstanceConstructor` initializer calls `SecurityContext.BeginInitialization( this )` before the code of the constructors.
* An `AfterLastInstanceConstructor` initializer calls `SecurityContext.EndConstruction( this )` at the end of the last constructor of the chain, from the `OnConstructed` method that Metalama introduces.
* An `AfterObjectInitializer` initializer calls `SecurityContext.EndInitialization( this )` from the `Initialize` method that Metalama introduces. Metalama rewrites each `new` expression of a securable class to call `Initialize` after the object initializer, so `new Folder { Owner = creator }` is not checked either.

`AccessGuard.Demand` applies these rules before it calls the policy. An access that the creator makes during the initialization is not cached, and it is audited like a granted access. The accesses to other objects are checked as usual: a constructor that reads another object needs the permissions of the subject on that object.

The rights of the creator are recorded in the current execution context, not on the object:

* A grant shares the objects being initialized with its parent, because it keeps the subject. `SecurityContext.Use` does not, because it changes the subject.
* Nothing is recorded in a system scope, where nothing is checked anyway. Entity Framework creates the objects that it loads in a system scope, so loading an object does not open it.
* Nothing is recorded without an execution context, so a constructor that sets secured members without a context fails closed.

Metalama calls `Initialize` only at the call sites that it compiles. An object created by reflection, by a serializer, or by an assembly that is not compiled with Metalama stays in the second state: its creator can set its properties, but nobody can read it or call its methods. The code that creates such an object calls `ISecurable.CompleteInitialization`, which calls `Initialize`. `ISecurable.IsInitialized` tells whether the initialization has ended. Both are default implementations of the interface, so a class does not implement them. The interface does not derive from `IInitializable`: every class that implements it directly would then have to implement `Initialize` in its source code, because Metalama introduces this method after the C# compiler has checked the source.

The `AfterLastInstanceConstructor` initializer adds an `InitializationContext` parameter to the constructors, which coordinates the initializers across the levels of a class hierarchy. A parameterless constructor is kept, and forwards to the constructor that has the parameter, so `Activator.CreateInstance<T>()` and the `new()` constraint still work. A constructor with parameters gets an optional `InitializationContext` parameter, so reflection code that passes arguments must pass this parameter too.

The introduced `Initialize` method is public, and `OnConstructed` is protected. `[SecurableType]` excludes both from default securing, because a check would fail for every creator who cannot write the object.

> [!NOTE]
> Metalama rewrites a `new` expression of a securable class even inside an expression tree, where the rewritten call does not compile (error `CS0854`). The value converter of `Invoice.Notes` in `InvoicingDbContext` therefore creates its lists in static methods, and not in its lambda expressions.

### ISecurable and SecurableType

`ISecurable` is an empty interface that marks the securable classes. The `[SecurableType]` aspect is applied to this interface, and Metalama propagates an `[Inheritable]` aspect from an interface to every class that implements it, and to their derived classes. `IRoleBasedSecurable` derives from `ISecurable`, so the demo's `Entity`, which derives from `RoleBasedSecurable`, has no attribute at all:

[!metalama-file Metalama.Samples.Authorization/Framework/ISecurable.cs]

The `[SecurableType]` aspect adds `[RequiresPermission]` to all members of a class that do not have it yet, and registers a validator (see below). On the interface itself, the aspect does nothing: its eligibility rejects interfaces except for inheritance, with `builder.ExceptForInheritance().MustNotBeInterface()`. Its eligibility also rejects a class that does not implement `ISecurable`, because the checks are made on the current instance.

Attaching the aspect to the interface, rather than asking the developer to add an attribute to each class hierarchy, makes two facts the same: an object that the policy can secure is always secured and validated. With an attribute, a new hierarchy of securable classes that forgets the attribute has no default check, and the validator cannot report it, because the validator is part of the missing aspect. With the interface, the security of a class is part of its type, so it cannot be forgotten. The interface also gives the parameter checks a type: a `[RequiresPermission]` parameter must be securable, which the compiler verifies.

The cost is that securing becomes implicit: a class that implements the interface for another reason is secured too. A type or a member is excluded explicitly with `[AuthorizationOptions( IsDefaultSecuringEnabled = false )]`. The framework uses this for its own infrastructure members, such as `RoleBasedSecurable.HasRole`, which the policy calls, and for `SecurableList<T>` (see [Collections](#collections)). The framework unit tests disable default securing for their whole assembly, because they test explicit attributes.

[!metalama-file Metalama.Samples.Authorization/Framework/SecurableTypeAttribute.cs]

The aspect adds child aspects with `builder.Outbound.SelectMany(...).AddAspectIfEligible(...)`. A child aspect must be processed after its parent, so the library declares the order with `[assembly: AspectOrder]` (see [GrantsPermissions](#grantspermissions)).

The aspect secures by default. Every field and every property gets a check, whatever its accessibility, because they hold the state of the object: a member of the class must not read or change the state without the permission. Every public method gets a check too, and so does every explicit interface implementation: such a member is private in C#, but any caller reaches it through the interface. A reader who copies the aspect without any configuration therefore gets a secured class, not a class with unsecured methods. A member that must stay unsecured is excluded explicitly, with `[AuthorizationOptions( IsDefaultSecuringEnabled = false )]`, which also documents the decision for a reviewer.

The aspect never secures private methods, members that already have `[RequiresPermission]`, or overrides of `ToString`, `Equals` and `GetHashCode`. Loggers and collections call these methods. A private method runs with the permissions of its caller, and its accesses to the state are checked. A `readonly` field gets only a read check, because only a constructor can write it.

### Verifying that every member is secured

Default securing has gaps by construction. The aspects cannot secure a static member, which has no instance on which to check a permission, nor an indexer or an event, which they do not support. A member can also lose its check by mistake, for instance after a change of options. In an authorization framework, such a gap is a security hole that nothing reports: the code compiles, the tests of the other members pass, and the member is open to every user.

For this reason, `[SecurableType]` also registers a validator. The validator runs after all aspects have been applied (`DeclarationValidationTime.AfterAllAspects`), so it sees the final code: the checks added by default, by explicit attributes and by inheritance. It reports a warning for each member that a caller can reach but that has no check and was not excluded explicitly:

* `AUTH001`: an exposed member has no permission check, for instance an indexer or an event.
* `AUTH002`: an exposed member is static.
* `AUTH003`: an exposed field or property has a mutable collection type that the aspect cannot protect, such as `List<T>`, an array or a dictionary (see [Collections](#collections)).

The validator turns the rule "every member of a secured class is secured" from a convention into a build-time verification, at no cost at run time. Each warning names the member and gives the two possible fixes. An explicit exclusion is visible in the code and in a review, while an unintended gap is not.

[!metalama-test ../tests/Metalama.Samples.Authorization.Tests/Validation_UnsecuredMembers.cs]

Two details of the validator come from the way Metalama transforms the code. To secure a field, Metalama turns it into a property, so the validator looks for the aspect on both the field and the property (`IProperty.OriginalField`). An abstract member has no body, so the validator checks its overrides instead.

### Collections

A check on a getter protects the reference that it returns, not the content of the object behind it. If a property returned an ordinary list, a user who can only read the property could add or remove items, which bypasses the write permission. The developer should not have to think about it: a collection property is written as ordinary C# code, and the framework protects it.

```csharp
public IList<InvoiceLine> Lines { get; } = new SecurableList<InvoiceLine>();
```

`SecurableList<T>` is the list itself, not a wrapper around another list. It is securable: a member of this type protects itself, so the validator accepts it. Its own members are excluded from default securing, because each change is checked against the owner of the list, not against the list. It derives from `Collection<T>`, and each change (`Add`, `Insert`, `Remove`, `RemoveAt`, `Clear` and the indexer setter) demands the write permission of the property on the owner of the list. The access kind of the check is `AccessKind.Add`, `AccessKind.Remove` or `AccessKind.Replace`, so audit records and denial messages say which change was refused, for instance `Cannot add an item to Invoice.Lines`. `AccessKind` is a set of flags, and `AccessKind.Change` selects every kind of change, which an auditor can use as a filter. The list checks the subject at the moment of the change, not the subject that read the property.

The owner is set on the first access. A property initializer such as `= new SecurableList<InvoiceLine>()` runs where the owner is not available, so the list starts without an owner. The getter generated by `[RequiresPermission]` knows the owner, so it calls `SecurableCollection.SetOwner` and passes `this`, the write permission and the property name. The owner can be set only once: setting the same owner again has no effect, and another owner makes the call throw, because a list shared by two objects would be checked against the wrong one. A list without an owner checks nothing. Before the first access, only the code of the owner can reach the list, and so can Entity Framework, which fills it when it loads the object.

The getter handles each declared type as follows:

* `IList<T>`, `ICollection<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `IEnumerable<T>` and `SecurableList<T>`: the getter sets the owner of the securable list and returns it. A read-only type is secured too, so casting it back to `IList<T>` does not bypass the check. If the property returns an ordinary mutable collection, such as a `List<T>`, the getter throws: it fails closed instead of returning an unprotected list. A sequence that is not a collection, such as a LINQ query, is returned as it is.
* An immutable type, such as `ImmutableArray<T>`, is returned as it is.
* A type that the getter cannot secure, such as `List<T>`, an array, a set or a dictionary, gets the warning `AUTH003`.

[!metalama-file Metalama.Samples.Authorization/Framework/SecurableList.cs]

Business rules on collections go to the owner. After the permission check and before each change, the securable list calls `ISecurableCollectionOwner.OnCollectionChanging` on its owner, with the name of the property and the kind of change. The owner throws to cancel the change. `Invoice` uses it to refuse changes of the lines of an invoice that is not a draft, while its notes can change at any status:

```csharp
void ISecurableCollectionOwner.OnCollectionChanging( string propertyName, AccessKind accessKind )
{
    if ( propertyName == nameof(this.Lines) )
    {
        this.EnsureStatus( InvoiceStatus.Draft );
    }
}
```

[!metalama-test ../tests/Metalama.Samples.Authorization.Tests/CollectionProperty.cs]

### Writing a securable class

The aspects check the accesses to the members of a class. They cannot check what the class does with the references that it returns or receives. A securable class therefore follows these rules, which the demo applies:

* A collection property is initialized with a `SecurableList<T>`. The getter fails closed if it returns an ordinary list, and the validator warns about collection types that it cannot secure.
* A reference to another secured object changes only through a method with a parameter check. A setter can only check the object that it belongs to, not the new value. So `Invoice.Customer` and `Customer.Department` have private setters, which only the class can call, and `Invoice.ChangeCustomer` and `Customer.MoveTo` check the permission on the new value. The private setters are checked too.
* A guarantee that does not depend on the policy is structural. `Company.Name` and `Department.Name` are init-only, so no rule added to the policy later can allow renaming them.
* A label identifies the object in messages without disclosing its data (see `IHasSecurityLabel` below).

## Build-time configuration with hierarchical options

The aspects read their defaults from the `AuthorizationOptions` class, which implements `IHierarchicalOptions<T>` for the compilation, namespaces, types, members and parameters:

| Option | Default | Effect |
|---|---|---|
| `DefaultReadPermission` | `Read` | Permission to read a field or property. |
| `DefaultWritePermission` | `Write` | Permission to write a field or property. |
| `DefaultExecutePermission` | `Write` | Permission to execute a method. |
| `SecureNonPublicMembers` | `false` | Whether `[SecurableType]` also secures `internal` and `protected` methods. Fields and properties are always secured. |
| `IsDefaultSecuringEnabled` | `true` | Whether `[SecurableType]` secures the target. Set it to `false` to exclude a namespace, a type or a member. |

[!metalama-file Metalama.Samples.Authorization/Framework/AuthorizationOptions.cs]

Options can be set at any level, and the nearest level wins. The demo uses all of them:

* A project fabric also secures non-public methods:

  [!metalama-file Metalama.Samples.Authorization.Demo/AuthorizationFabric.cs]

* A namespace fabric makes finance data confidential. In the `Finance` namespace, reading any member requires `ReadSensitive`. The `Budget` class has no attribute at all:

  [!metalama-file Metalama.Samples.Authorization.Demo/Finance/FinanceFabric.cs]

* The `[AuthorizationOptions]` attribute, which implements `IHierarchicalOptionsProvider`, sets options on a type or a member. On `Customer`, writing any property requires `Approve`, because commercial terms are a management decision. The contact details override this back to `Write`:

  [!metalama-file Metalama.Samples.Authorization.Demo/Sales/Customer.cs]

* Options are inherited by overriding members. The library excludes the abstract `RoleBasedSecurable.SecurityParent` property from default securing, and the exclusion also applies to its overrides in the demo project.

Options decide which checks the aspects generate, at build time. The policy decides whether a check passes, at run time. This is why the role-permission rules are not options: in a real application, they are data that an administrator changes without rebuilding the application.

> [!NOTE]
> The options class does not provide its defaults through `IHierarchicalOptions.GetDefaultOptions`. The options of a base type, including its defaults, override the namespace options of a derived type. A default `Read` on `Entity` would then cancel the `ReadSensitive` of the `Finance` namespace for `Budget`. Instead, the aspects apply the default when the merged value is still `null`.

## The abstract layer at run time

The generated code calls `AccessGuard.Demand`:

[!metalama-file Metalama.Samples.Authorization/Framework/AccessGuard.cs]

* There is no check in a system scope, opened with `SecurityContext.RunAsSystem()`. Trusted infrastructure code uses it when no user operation is running: the demo uses it to seed the database and to create the entities that Entity Framework loads, and `AccessGuard` uses it to run the policy. A member that needs more permissions on its own object, or on an object that it receives, uses `[GrantsPermissions]` instead (see [GrantsPermissions](#grantspermissions)).
* Without a current context, the access fails. A missing context is a configuration error, not a reason to skip security.
* The policy runs in a system scope. The policy reads secured members, such as the owner of the object, and these reads must not trigger checks recursively.
* The auditor runs outside of the system scope. An auditor that reads data or writes to a secured store does it with the permissions of the current subject, not with implicit privileges.
* An access to an object that is being initialized is not checked (see [Constructing an object](#constructing-an-object)).
* `AccessGuard.Deny` refuses an operation for a reason that is not a missing permission, and audits it like any other denial. The role-based layer uses it to refuse that a user assigns a role to themself, revokes a role from themself, or makes themself the owner of an object.
* The decision is cached in the current execution context. Each access is still audited, because the auditor records accesses, not decisions.
* A granted access with `ChangeOwnership` or `ManageRoles` changes the data that the role-based policy reads. After it, `AccessGuard` clears the cache of the current execution context and of the execution contexts that contain it, and removes the decisions on the object from the shared cache (see [Caching the decisions](#caching-the-decisions)).

### Caching the decisions

Every access to a field or a property runs a check, so the same decision is needed many times. For instance, `Invoice.Approve` reads `Status` and `Lines` and writes `Status`. The framework caches the decisions at two levels.

The first level is the execution context. `SecurityContext.Use` starts an execution context, for instance for a request. All the code that runs in the execution context shares its cache: the methods that it calls, the continuations of its `async` methods and the tasks that it starts. A method does not start an execution context when it enters an object. The check of the method has already put the object in the cache of the current execution context.

* The cache is an immutable dictionary, `ImmutableDictionary<ISecurable, ImmutableList<(Permission, bool)>>`. It compares the objects by reference, and it contains the denials as well as the grants.
* The `AsyncLocal<T>` field holds a reference to the execution context, and a new decision replaces the dictionary of the execution context with `ImmutableInterlocked`. The value of the `AsyncLocal<T>` field does not change. A decision cached by an `async` method is therefore visible to its caller after the method returns, and concurrent tasks add decisions without a lock.
* A grant breaks the execution context. `SecurityContext.Grant` starts a child execution context with an empty cache, because the decisions of the parent do not take the grant into account. When the grant ends, the parent execution context is restored, without the decisions of the child, which took the grant into account.
* The cache is cleared only after an access that changes the rights: a granted `ChangeOwnership` or `ManageRoles`. The access is checked before the change, so the next checks of the operation see the new rights. Any other change of the rights applies to the execution contexts that start after it. A long-running operation, such as a background job, keeps the decisions of its start. It should start an execution context for each unit of work.

The second level is shared by all the execution contexts of the process. `CachingAuthorizationPolicy` wraps a policy and caches its decisions in an `IMemoryCache`. It is called only when the cache of the execution context does not contain the decision, so it does not change the consistency of an operation.

* The keys identify the subject and the object, not their instances, because each request loads new instances. The demo uses the class and the database key of each entity. An object without a key, for instance an invoice that has not been saved, is not cached.
* The cache is invalidated manually. `Invalidate` removes the decisions on an object, and on the objects that depend on it, for instance the objects that inherit their roles from it. `InvalidateAll` removes all decisions. Each object has a cancellation token, stored in the memory cache too, so that the tokens of the objects that are no longer used are removed. When a token is evicted, it is canceled, so no decision survives the token that invalidates it.
* The tokens are taken before the policy runs. When an invalidation happens while the policy runs, the new entry has a canceled token and expires immediately.
* A decision is shared only when the data that it was made from is current. Each invalidation increments `CachingAuthorizationPolicy.Generation`, and the data access layer calls `RecordLoad` when it loads an object. A decision on an object, or on one of its dependencies, that was loaded before the last invalidation may come from data that has changed since then, so it is not shared. Without this rule, a request that loaded an invoice before another request gave it to a new owner would cache the decision of the former owner under a new token, and every other request would see it until it expires. A decision on an object whose load was not recorded is not shared either.
* A decision also expires after five minutes, even without invalidation.

[!metalama-file Metalama.Samples.Authorization/Framework/CachingAuthorizationPolicy.cs]

In the demo, `PermissionCacheInvalidationInterceptor` invalidates the cache when a change is visible to the other requests: after the save, or, when the save runs in an explicit transaction, after the commit. A rolled-back transaction invalidates nothing. A changed or deleted entity is invalidated, because its owner or its parent may have changed. A change of the role assignments invalidates all the decisions, because the roles of an object apply to all its descendants, and role assignments change rarely. `DatabaseAuthorizationPolicy` raises its `Reloaded` event after it reloads the rules, and the cache is cleared then. `SystemScopeMaterializationInterceptor` records the load of each entity.

The two interceptors are single instances. Entity Framework creates an internal service provider for each set of interceptor instances, so an instance for each application would create one per test. They find the cache through `InvoicingDbContext.CachingPolicy`.

[!metalama-file Metalama.Samples.Authorization.Demo/Data/PermissionCacheInvalidationInterceptor.cs]

### What the messages and audit records disclose

A denial message is shown to a user who does not have access to the object, and audit records end up in log files. Both must not disclose the data that the check protects. `AccessDeniedException` and `AccessRecord` therefore do not reference the securable object: they contain only its label. A class implements `IHasSecurityLabel` to provide a label made of non-sensitive data, typically its type name and its identifier, such as `Customer #1`. Other objects are labeled with their type name. A message never calls `ToString`, which often returns a name, and a logger that serializes the exception cannot reach the secured members.

### What the auditor records

The auditor receives every denial. It can also select permissions whose grants it records, with `IAccessAuditor.AuditsGrantsOf`. The demo records the grants of `ManageRoles` and `ChangeOwnership`, because these operations change who can do what: an audit trail of denials alone would not show who gave a right to whom.

### Trust boundary

The framework enforces authorization inside a process. It protects against missing checks, not against malicious code: any code of the process can call `SecurityContext.Use`, `SecurityContext.Grant` or `SecurityContext.RunAsSystem`. Only trusted infrastructure calls them, such as the authentication middleware or the database seeder. `SecurityContext.Current` has no public setter, so the only way to change it is a scope that restores the previous value. A context or a system scope also applies to the tasks started while it is active, because `AsyncLocal<T>` values flow into them. `SecurityContext.Use` ends a system scope until its own scope is disposed: switching to a subject drops the privileges of the system scope.

`SecurityContext` stores the current execution context, which holds the context, the grants, the cache and the objects being initialized, and the system scope in `AsyncLocal<T>` fields. They follow `async` code and stay isolated between concurrent requests. The scopes are `SecurityScope` structures, so opening one does not allocate an object. Disposing the default value of the structure does nothing.

[!metalama-file Metalama.Samples.Authorization/Framework/SecurityContext.cs]

## The role-based policy

`RoleBasedAuthorizationPolicy` stores the rules by class. `IsGranted` walks the class hierarchy of the securable. For each rule that matches the permission and a role of the subject, a `Deny` rule returns `false` immediately, and an `Allow` rule records the grant. `PermissionEffect.None` is the default value of the enumeration. A rule with this effect is ignored, so an uninitialized rule never allows anything.

A `RoleBasedAuthorizationPolicyBuilder` collects the rules, and its `Build` method creates an immutable policy, which concurrent requests can use without locks. To change the rules, an application builds a new policy and replaces the old instance. The builder rejects a rule on an interface: the policy walks base classes, never interfaces, so such a rule would never match.

[!metalama-file Metalama.Samples.Authorization/RoleBased/RoleBasedAuthorizationPolicy.cs]

`RoleBasedSecurable` resolves roles. There are two kinds of roles:

* An assigned role is stored on an object, and the children of this object inherit it. `HasRole` checks the assignments of the object, then delegates to `HasRole` of the security parent, so any implementation of `IRoleBasedSecurable` can be a parent.
* A computed role is never assigned: the object computes it from its own data, and children do not inherit it. `Everyone` matches every subject, and `Owner` matches the owner of the object. An application defines more computed roles with `Role.Computed` and resolves them by overriding `HasComputedRole`. The demo defines `Creator`, the person who created an invoice.

Nobody can assign a role to themself. Without this rule, anyone who manages roles could give themself any right.

`RoleBasedSecurable` does not store anything. The `Owner` property is abstract, and derived classes store the role assignments through the abstract `GetRoleAssignments`, `AddRoleAssignment` and `RemoveRoleAssignment` methods. The framework ships no storage: the demo stores them with Entity Framework in `EfRoleBasedSecurable`, and the framework tests use a small in-memory implementation. The `[RequiresPermission]` aspect on the abstract `Owner` property is inherited, so every implementation of `Owner` is secured.

[!metalama-file Metalama.Samples.Authorization/RoleBased/RoleBasedSecurable.cs]

## The demo: invoicing at Contoso

The company Contoso has three departments: Sales, Marketing and Engineering. Departments manage customers, issue invoices and have a yearly budget. An invoice moves through the statuses Draft, Approved, Sent, Paid and Voided.

### The security model from the business point of view

The [security model of the demo](Metalama.Samples.Authorization.Demo/README.md) describes who may do what, in the words of the business, without reference to the code: the positions, and the rules on invoices, customers, budgets and access rights. Each rule has an identifier, such as `INV-4`, which also appears in the permission matrix of the tests.

### How the demo implements the model

The positions are roles assigned on the company or on a department, and inherited by the objects below. The owner, the creator and the "every user" cases are computed roles: `Owner` and `Everyone` are built in, and the demo defines `Creator`:

| Role | Scope | Responsibilities |
|---|---|---|
| Department manager | department | Approves the invoices of the department, maintains customers and their credit terms, sees sensitive customer data and the department budget, moves invoices between departments they manage, assigns roles in the department. |
| Department assistant | department | Prepares and sends invoices, maintains customer contact details. |
| Accountant | company | Records payments, voids invoices, manages budgets. Does not edit invoice content. |
| Auditor | company | Reads everything, including sensitive data. Every change is denied. |
| Administrator | company | Assigns and revokes roles everywhere. Sensitive data is denied. |
| Owner (computed) | object | Reads and edits the invoice they own, but cannot approve it. |
| Creator (computed) | invoice | Cannot approve the invoice they created, even after giving it to another owner. |
| Everyone (computed) | all | Reads the company directory. |

The sample data contains one person per typical combination of positions:

| User | Positions |
|---|---|
| Alice | Administrator |
| Ada | Accountant |
| Otto | Auditor |
| Grace | Auditor, and manager of Marketing |
| Mia | Manager of Sales and of Marketing, owner of the marketing invoice |
| Sam | Assistant in Sales, owner of the sales invoice |
| Erin | Manager of Engineering |
| Tom | Assistant in Engineering, owner of the engineering invoice |
| Vera | No position |

The policy expresses three common security principles with `Deny` rules:

* Four-eyes principle: neither the owner nor the creator of an invoice can approve it, even as a department manager. The creator rule is needed because a manager can change the owner of an invoice, but nobody can change its creator.
* Segregation of duties: an auditor cannot change anything, even with a manager role in a department.
* Least privilege: administrators manage access but cannot read sensitive data.

The rules are data, not code. They are stored in the database, where an administrator can change them, and `DemoPolicy` contains only the seed data of the database:

[!metalama-file Metalama.Samples.Authorization.Demo/DemoPolicy.cs]

Each rule is a `PermissionRuleRecord`, which stores the class, the permission and the role by name, and the effect as text. The rule on all classes stores `*` as its class.

`DatabaseAuthorizationPolicy` is the policy of the application. It loads the rules from the database with `ReloadAsync`, and builds an immutable `RoleBasedAuthorizationPolicy` with the non-generic `RoleBasedAuthorizationPolicyBuilder.Add` method. After an administrator changes the rules, a new call to `ReloadAsync` replaces the policy, and the change applies to the next check of every request. The policy is fail-closed in two ways:

* Before the rules are loaded, every check is denied.
* A rule on an unknown class, an unknown permission or an unknown role makes the reload fail, and the previous rules stay in force. A mistyped name would match no object, no check or no subject, so loading the rule could silently remove a `Deny` rule. For instance, a rule stored as "Deny Write to Auditer" would let the auditors write.

A role is resolved from its name by `DemoRoles.FromName`, which knows the roles of the application, and a permission must be a constant of `PermissionNames` or `DemoPermissions`. A new role therefore requires a change of `DemoRoles`.

[!metalama-file Metalama.Samples.Authorization.Demo/Data/DatabaseAuthorizationPolicy.cs]

The rule records are not securable objects in this example: no user operation of the demo changes them. In a real application, editing the rules is the most privileged operation of all, and it would be protected and audited like the role assignments.

Authorization is a different concern from business rules. The policy answers "may this user do this?". The `Invoice` methods enforce rules such as "an approved invoice cannot be edited", and they throw `InvalidOperationException`, not `AccessDeniedException`.

Moving an invoice to another department needs a parameter check. `Invoice.TransferTo` requires `ChangeOwnership` on the invoice and on the destination department. Without the second check, a user could move an invoice into a department where they have more rights.

The program runs each scenario as a separate request of a user, through the application services, and prints the outcome. The auditor writes the denials, and the grants of the permissions that change rights, to the log. The messages name objects only by their label. Here is an excerpt:

```text
Approving: the four-eyes principle and segregation of duties
  GOOD Mia approves Sam's invoice: succeeded
  GOOD Mia adds a line to the approved invoice (business rule): failed: Invoice SAL-0001 is Approved, expected Draft.
warn: Metalama.Samples.Authorization.Demo.Services.LoggingAccessAuditor[277457468] Mia was denied Approve on Invoice.Approve of Invoice #2.
  GOOD Mia approves her own marketing invoice: was denied: Cannot execute Invoice.Approve: 'Mia' does not have the Approve permission on Invoice #2.
info: Metalama.Samples.Authorization.Demo.Services.LoggingAccessAuditor[1113576134] Mia was granted ChangeOwnership on Entity.Owner of Invoice #2.
  GOOD Mia gives her marketing invoice to Grace: succeeded
warn: Metalama.Samples.Authorization.Demo.Services.LoggingAccessAuditor[277457468] Mia was denied Approve on Invoice.Approve of Invoice #2.
  GOOD Mia approves the invoice that she created, now owned by Grace: was denied: Cannot execute Invoice.Approve: 'Mia' does not have the Approve permission on Invoice #2.
...
Moving an invoice: checking the parameter
  GOOD Mia creates a sales invoice: succeeded
info: Metalama.Samples.Authorization.Demo.Services.LoggingAccessAuditor[1113576134] Mia was granted ChangeOwnership on Invoice.TransferTo(target) of Department #2.
info: Metalama.Samples.Authorization.Demo.Services.LoggingAccessAuditor[1113576134] Mia was granted ChangeOwnership on Invoice.TransferTo of Invoice #4.
  GOOD Mia moves it to Marketing, which she also manages: succeeded
warn: Metalama.Samples.Authorization.Demo.Services.LoggingAccessAuditor[277457468] Mia was denied ChangeOwnership on Invoice.TransferTo(target) of Department #3.
  GOOD Mia moves it to Engineering: was denied: Cannot execute Invoice.TransferTo(target): 'Mia' does not have the ChangeOwnership permission on Department #3.
```

## Persistence and dependency injection

The demo is structured like a real application. The domain objects are stored with Entity Framework Core (code first) in an in-memory SQLite database, and the application services are registered in a dependency injection container:

[!metalama-file Metalama.Samples.Authorization.Demo/Services/ServiceCollectionExtensions.cs]

The services, such as `InvoiceService`, load the objects, call the domain methods, and save the changes. They contain no permission check: the domain objects enforce them, whichever code calls them. A service checks a permission explicitly only for an operation that has no secured object. For instance, `StaffService.CreateUserAsync` calls `AccessGuard.Demand` on the company, because a user account is not a securable object.

`RequestRunner` plays the role of the authentication middleware of a web application. It creates a dependency injection scope, which has its own database context, loads the user, and sets `SecurityContext.Current` while the request runs:

[!metalama-file Metalama.Samples.Authorization.Demo/Services/RequestRunner.cs]

`SecurityContext.Current` is stored in an `AsyncLocal<T>`. A value set inside an awaited method is not visible to the caller of this method, so the context must be set in the method that awaits the request, as `RequestRunner` does.

### Entity Framework and the secured members

Entity Framework reads and writes every mapped property when it loads an object, detects changes, and saves them. These operations belong to the infrastructure, not to the user, and they often run without a security context. They must not go through the secured accessors. The demo follows these rules:

* The model uses `PropertyAccessMode.Field`. Entity Framework reads and writes the backing fields directly, including the fields that Metalama generates when it overrides an auto-property.
* `SystemScopeMaterializationInterceptor` creates each entity in a system scope, with its parameterless constructor, which can be private, and records its load for the shared cache of decisions. Constructors can therefore set secured properties, for instance by chaining to the constructor that the application uses. The scope is closed by a `using` statement, so it cannot remain open after a failure.
* Entity Framework fills collections through their backing field, before the first access through the property, so a `SecurableList<T>` has no owner yet and checks nothing. A primitive collection, such as `Invoice.Notes`, is assigned by Entity Framework when it loads the object, so a value converter recreates a `SecurableList<T>`.
* `EfRoleBasedSecurable`, the base class of `Entity`, stores the owner as a `User` navigation and the role assignments as an owned collection of `RoleAssignmentRecord`. `ConfigureRoleBasedStorage` maps them for each entity class. These storage members are excluded from default securing with `[AuthorizationOptions( IsDefaultSecuringEnabled = false )]`. The public `Owner` and `RoleAssignments` members are the secured views of these members, and the model ignores them.
* The navigations that the security model needs are loaded automatically with `AutoInclude`: the parent chain of each object, its owner, and its role assignments. Otherwise, a role inherited from a parent that is not loaded would be missed. The policy would then deny the access, which is safe but wrong.
* The state of an entity is stored in properties, not in fields. Metalama secures a field by turning it into a property, and moves its value to a new field, for instance `_amount1` for `_amount`, which Entity Framework cannot discover by convention. A property with a private setter, such as `Budget.Amount`, has a backing field that Entity Framework discovers, `_amount`, and it is secured in the same way.
* By convention, Entity Framework maps only the properties that have a setter. The get-only properties that expose the state of an object, such as `Invoice.Status`, are mapped explicitly. A test checks that every field of every entity is mapped.

[!metalama-file Metalama.Samples.Authorization.Demo/Data/InvoicingDbContext.cs]

A connection to an in-memory SQLite database cannot be used by concurrent requests. `InMemoryDatabase` keeps a named, shared-cache in-memory database open, and each database context opens its own connection to it.

## Tests

### Aspect tests

The aspect tests in `Metalama.Samples.Authorization.Tests` verify the generated code. They cover each kind of target (methods, `async` methods, properties, `init` accessors, fields, explicit interface implementations, method and constructor parameters), default permissions, inheritance by overrides and by abstract members, each level of options, the eligibility errors, the securable lists of collection properties, the warnings of the validator, and the grants of `[GrantsPermissions]`: their position inside the permission check, on members and on parameters, their inheritance by overrides and by implementations of an interface, their eligibility, and the error `AUTH004` on a member or a parameter without a check.

`SecurableInterface` shows that implementing `ISecurable`, directly or through a derived interface, is enough. Two tests, `Demo_SalesConventions` and `Demo_FinanceConventions`, compile the real configuration of the demo application with `// @Include` directives: the `Entity` base class and its storage types, the project fabric, the namespace fabric and the permission names. They show the code that this configuration generates. For instance, in the `Finance` namespace:

[!metalama-compare ../tests/Metalama.Samples.Authorization.Tests/Demo_FinanceConventions.cs]

### Unit tests of the framework

`Metalama.Samples.Authorization.UnitTests` tests the framework on small test types, independently of the demo:

* `SecurityContextTests`: fail-closed behavior, system scopes, `Use` that ends a system scope, and context isolation across `await` and concurrent tasks.
* `InitializationTests`: unlimited rights on an object while its constructors and its object initializer run, the checks on other objects, the checks after the initialization, a constructor without a context that fails closed, an object created by reflection that is unusable until `CompleteInitialization`, and the parameterless constructor of a class that implements `ISecurable` directly.
* `NestedAccessTests`: the permission of a member does not grant the permissions of the members that it accesses, including private state.
* `PermissionCacheTests`: the cache of an execution context, shared by the `async` methods and the tasks of the context, the denials cached and still audited, the grant that breaks the execution context, a change of rights that applies to the next execution context, and a change of owner that clears the cache at once.
* `CachingAuthorizationPolicyTests`: the cache shared by all execution contexts, the objects without a key or without a recorded load, the objects loaded before an invalidation, the invalidation of an object and of its descendants, and an invalidation while the policy runs.
* `GrantsPermissionsTests`: a grant gives its permissions after the check of the member, on the current instance only, in the current context only, and until the member returns, also across `await`. It takes precedence over the denials of the policy.
* `AccessGuardTests`: audit records of denials and grants, labels in messages, and an auditor that runs without privileges.
* `SecurableListTests`: the owner set on the first access and only once, changes denied without the write permission, with the kind of change, read-only properties secured too, the getter that fails closed on an ordinary list, and the owner hook.
* `RoleBasedPolicyTests`: allow and deny rules, class hierarchy, built-in and application-defined computed roles, role inheritance through any parent, self-assignment, self-revocation, self-ownership except for the creator of an object, and the immutability of the policy.

### Unit tests of the demo security model

`Metalama.Samples.Authorization.Demo.UnitTests` tests the security model of the invoicing application.

Its core is a permission matrix. Each row is an operation of the application with the set of users allowed to perform it:

```csharp
Op( "INV-5: Approve a sales invoice", org => org.SalesInvoice.Approve(), Mia ),
Op( "CUS-3: Read a customer tax ID", org => _ = org.Fabrikam.TaxId, Mia, Ada, Otto, Grace ),
```

Each row starts with the identifier of the business requirement that it checks (see the [security model of the demo](Metalama.Samples.Authorization.Demo/README.md)), so the name of each test case gives the requirement, the operation, the user and the expected outcome.

The `PermissionMatrixTests` theory runs every operation as every user, on a fresh database. The objects are loaded from the database before the operation. The operation must succeed for the users of the row and throw `AccessDeniedException` for all the others. After a successful operation, the test saves the changes as the user, which must not trigger any check. Each row therefore checks both the grants and the denials, and adding a row is enough to cover a new operation. The rows describe the intended business rules, independently of how `DemoPolicy` implements them. The matrix found a gap during the development of this example: an auditor who was also a department manager could create invoices.

The other test classes cover what a single operation cannot show:

* `WorkflowTests`: the invoice lifecycle, the creator who becomes the owner, the users who cannot make themselves the owner or revoke their own role, role changes that take effect once they are saved, transfers that change who inherits access, and the difference between a denied permission and a broken business rule.
* `OptionsBehaviorTests`: proof at run time that each build-time option took effect.
* `StructuralRulesTests`: the guarantees that do not depend on the policy, such as init-only names, private setters for references to secured objects, and read-only collections.
* `AuditTests`: the content of audit records for the demo operations.
* `PolicyStorageTests`: the rules are stored in the database, a changed rule takes effect after a reload, a rule on an unknown class, permission or role makes the reload fail without changing the policy, and a policy without rules denies everything.
* `SharedCacheTests`: a request that works on data loaded before a change does not share its decisions, and a save in a transaction invalidates the shared cache after the commit, and not after a rollback.
* `PersistenceTests`: loading and saving run no check; every field of every entity is mapped; authorized changes and role assignments are persisted and effective in a new database context; denied changes leave the database unchanged; queries are not filtered; and complete workflows run through the services as separate requests, including concurrent requests of different users.

## Limitations

* The cache of an execution context is cleared only after a change of the owner or of the role assignments. A long-running operation keeps the decisions of its start.
* The cache shared by all execution contexts is invalidated after a save, in the process that saves. The other requests do not see a change that is not saved. Another instance of the application, or a change made directly in the database, is seen only when the decisions expire, after five minutes.
* An object created by reflection, by a serializer, or by an assembly that is not compiled with Metalama is unusable until its creator calls `ISecurable.CompleteInitialization`. In the meantime, the creator can still set its properties in the same execution context.
* Every access to a field or a property runs a check. A check that finds its decision in the cache of the execution context costs a dictionary lookup and the allocation of a `Permission`.
* Rules apply to types, not to object states. "Only draft invoices can be edited" is a business rule in the `Invoice` methods.
* A department manager can assign any role in their department to another person, including a role with more rights than their own. Nobody can assign a role to themself, but a real system also restricts which roles each role may assign.
* The aspects protect members, not rows. A user without any role can query invoices, but cannot read their members. To filter rows, use another mechanism, for instance an Entity Framework global query filter.
* The demo uses an in-memory database, created and seeded at startup, including the rules of the policy.
