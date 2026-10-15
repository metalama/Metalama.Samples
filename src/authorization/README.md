---
uid: sample-authorization
level: 300
summary: "This example enforces a role-based authorization model with Metalama aspects: permission checks on methods, properties, fields and parameters, default permissions inherited by all business objects, and build-time configuration with hierarchical options."
keywords: "authorization, permissions, role-based access control, RBAC, security, aspect, hierarchical options, fabrics, contracts"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Enforcing authorization with aspects

[!metalama-project-buttons Metalama.Samples.Authorization.Demo]

This example shows how to enforce a non-trivial authorization model in business objects without writing permission checks by hand. Aspects add the checks to methods, properties, fields and parameters. A run-time policy decides whether the current user has the required permission on the object being accessed.

> [!WARNING]
> This example is a proof of concept. It is not tested for production use. See [Limitations](#limitations).

## Two layers: an abstract framework and a role-based model

The framework has two layers.

The first layer is abstract. It does not assume any security model. It defines five concepts:

| Concept | Type | Description |
|---|---|---|
| Subject | `ISubject` | The user or service on whose behalf the code runs. |
| Permission | `Permission` | Something that the subject must have on an object, for instance `Read`, `Write` or `Approve`. Permissions are compared by name. |
| Securable | any object | The object on which the permission is required. For a method or property, it is the current instance. For a parameter, it is the argument. |
| Policy | `IAuthorizationPolicy` | Decides whether a subject has a permission on a securable. |
| Security context | `ISecurityContext` | The current subject, the policy, and an optional auditor. `SecurityContext.Current` exposes it. |

The aspects, the options and the run-time entry point `AccessGuard` belong to this layer. They only ask one question: does the current subject have this permission on this object? The answer comes from the `IAuthorizationPolicy.IsGranted` method, and the abstract layer does not know how the policy computes it.

The second layer is one implementation of `IAuthorizationPolicy`: the classic role-based access control model, in the `RoleBased` namespace.

* Roles are assigned to subjects on an object, for instance "Mia is department manager of Sales".
* Each object has a security parent and inherits the role assignments of its ancestors. For instance, an invoice inherits the role assignments of its department, and the department inherits those of the company.
* Rules allow or deny a permission to a role on a type of object, for instance "department managers are allowed to approve invoices". A rule on a base type also applies to derived types.
* A permission is granted when at least one rule allows it and no rule denies it.
* Two roles are built in and never assigned. Every subject has `Everyone`. The `Owner` of an object has `Owner` on this object only.

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
| `Metalama.Samples.Authorization.Demo` | A console application with an invoicing domain that uses the framework. |
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
* A private setter is not checked. The class changes its own state through it, typically from a method that is itself secured.
* The aspect is `[Inheritable]`. An override of a secured virtual member gets the same check, so a derived class cannot skip it by not calling the base implementation. On an abstract member, the aspect secures all the overrides.
* The eligibility rules reject static members, `out` parameters and parameters of a value type. Static members have no instance on which to check a permission. Abstract members are rejected except through inheritance, with `builder.ExceptForInheritance().MustNotBeAbstract()`.
* When the attribute does not name a permission, the defaults of `AuthorizationOptions` apply. When a method has no permission and no default is configured, the aspect reports the error `AUTH001`.

### ApplyDefaultPermissions

The `[ApplyDefaultPermissions]` aspect adds `[RequiresPermission]` to all members of a class that do not have it yet. The demo applies it to the `Entity` base class. The aspect is `[Inheritable]`, so it applies to all derived classes, including the classes of other projects.

[!metalama-file Metalama.Samples.Authorization/Framework/ApplyDefaultPermissionsAttribute.cs]

The aspect adds child aspects with `builder.Outbound.SelectMany(...).AddAspectIfEligible(...)`. A child aspect must be processed after its parent, so the library declares the order with `[assembly: AspectOrder]`.

The aspect never secures private members, static members, members that already have `[RequiresPermission]`, or overrides of `ToString`, `Equals` and `GetHashCode`. Exception messages and collections call these methods.

## Build-time configuration with hierarchical options

The aspects read their defaults from the `AuthorizationOptions` class, which implements `IHierarchicalOptions<T>` for the compilation, namespaces, types, members and parameters:

| Option | Default | Effect |
|---|---|---|
| `DefaultReadPermission` | `Read` | Permission to read a field or property. |
| `DefaultWritePermission` | `Write` | Permission to write a field or property. |
| `DefaultExecutePermission` | none | Permission to execute a method. Without a value, `[ApplyDefaultPermissions]` does not secure methods. |
| `SecureNonPublicMembers` | `false` | Whether `[ApplyDefaultPermissions]` also secures `internal` and `protected` members. |
| `IsDefaultSecuringEnabled` | `true` | Whether `[ApplyDefaultPermissions]` secures the target. Set it to `false` to exclude a namespace, a type or a member. |

[!metalama-file Metalama.Samples.Authorization/Framework/AuthorizationOptions.cs]

Options can be set at any level, and the nearest level wins. The demo uses all of them:

* A project fabric secures all methods with `Write`, and also secures non-public members:

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

* There is no check in a system scope, opened with `SecurityContext.RunAsSystem()`. Trusted code uses it, for instance to load data from a database.
* Without a current context, the access fails. A missing context is a configuration error, not a reason to skip security.
* The policy runs in a system scope. The policy reads secured members, such as the owner of the object, and these reads must not trigger checks recursively. The exception message is built in the same scope, because it calls `ToString`.
* On a denial, the auditor receives the exception before it is thrown.

`SecurityContext` stores the context and the system scope in `AsyncLocal<T>` fields. They follow `async` code and stay isolated between concurrent requests.

[!metalama-file Metalama.Samples.Authorization/Framework/SecurityContext.cs]

## The role-based policy

`RoleBasedAuthorizationPolicy` stores the rules by type. `IsGranted` walks the type hierarchy of the securable. For each rule that matches the permission and a role of the subject, a `Deny` rule returns `false` immediately, and an `Allow` rule records the grant.

[!metalama-file Metalama.Samples.Authorization/RoleBased/RoleBasedAuthorizationPolicy.cs]

`RoleBasedSecurable` resolves roles. `Everyone` matches every subject. `Owner` matches the owner of the object itself, and children do not inherit it. Assigned roles are searched on the object and on its security ancestors.

[!metalama-file Metalama.Samples.Authorization/RoleBased/RoleBasedSecurable.cs]

## The demo: invoicing at Contoso

The company Contoso has three departments: Sales, Marketing and Engineering. Departments manage customers, issue invoices and have a yearly budget. An invoice moves through the statuses Draft, Approved, Sent, Paid and Voided.

The roles are the following:

| Role | Scope | Responsibilities |
|---|---|---|
| Department manager | department | Approves the invoices of the department, maintains customers and their credit terms, sees sensitive customer data and the department budget, moves invoices between departments they manage, assigns roles in the department. |
| Department assistant | department | Prepares and sends invoices, maintains customer contact details. |
| Accountant | company | Records payments, voids invoices, manages budgets. Does not edit invoice content. |
| Auditor | company | Reads everything, including sensitive data. Every change is denied. |
| Administrator | company | Assigns and revokes roles everywhere. Sensitive data is denied. |
| Owner (built in) | object | Reads and edits the invoice they created, but cannot approve it. |
| Everyone (built in) | all | Reads the company directory. |

The policy expresses three common security principles with `Deny` rules:

* Four-eyes principle: the owner of an invoice cannot approve it, even as a department manager.
* Segregation of duties: an auditor cannot change anything, even with a manager role in a department.
* Least privilege: administrators manage access but cannot read sensitive data.

[!metalama-file Metalama.Samples.Authorization.Demo/DemoPolicy.cs]

Authorization is a different concern from business rules. The policy answers "may this user do this?". The `Invoice` methods enforce rules such as "an approved invoice cannot be edited", and they throw `InvalidOperationException`, not `AccessDeniedException`.

Moving an invoice to another department needs a parameter check. `Invoice.TransferTo` requires `ChangeOwnership` on the invoice and on the destination department. Without the second check, a user could move an invoice into a department where they have more rights.

The program signs in as different users and prints the outcome of each scenario. Here is an excerpt:

```text
Approving: the four-eyes principle and segregation of duties
  GOOD Mia approves Sam's invoice: succeeded
  GOOD Mia adds a line to the approved invoice (business rule): failed: Invoice SAL-0001 is Approved, expected Draft.
        [audit] Mia (sales and marketing manager) was denied Approve on Invoice.Approve.
  GOOD Mia approves her own marketing invoice: was denied: Cannot execute Invoice.Approve: 'Mia (sales and marketing manager)' does not have the Approve permission on 'invoice MKT-0001'.
        [audit] Grace (auditor, marketing manager) was denied Approve on Invoice.Approve.
  GOOD Grace, marketing manager but also auditor, approves the marketing invoice: was denied: Cannot execute Invoice.Approve: 'Grace (auditor, marketing manager)' does not have the Approve permission on 'invoice MKT-0001'.
...
Moving an invoice: checking the parameter
  GOOD Mia moves a draft from Sales to Marketing, which she also manages: succeeded
        [audit] Mia (sales and marketing manager) was denied ChangeOwnership on Invoice.TransferTo(target).
  GOOD Mia moves the draft to Engineering: was denied: Cannot execute Invoice.TransferTo(target): 'Mia (sales and marketing manager)' does not have the ChangeOwnership permission on 'Engineering'.
```

## Tests

### Aspect tests

The aspect tests in `Metalama.Samples.Authorization.Tests` verify the generated code. They cover each kind of target (methods, `async` methods, properties, `init` accessors, fields, method and constructor parameters), default permissions, inheritance by overrides and by abstract members, each level of options, and the eligibility errors.

Two tests, `Demo_SalesConventions` and `Demo_FinanceConventions`, compile the real configuration of the demo application with `// @Include` directives: the `Entity` base class, the project fabric, the namespace fabric and the permission names. They show the code that this configuration generates. For instance, in the `Finance` namespace:

[!metalama-compare ../tests/Metalama.Samples.Authorization.Tests/Demo_FinanceConventions.cs]

### Unit tests of the framework

`Metalama.Samples.Authorization.UnitTests` tests the framework on small test types, independently of the demo:

* `SecurityContextTests`: fail-closed behavior, system scopes, and context isolation across `await` and concurrent tasks.
* `AccessGuardTests`: audit records and exception messages.
* `RoleBasedPolicyTests`: allow and deny rules, type hierarchy, built-in roles, and role inheritance.

### Unit tests of the demo security model

`Metalama.Samples.Authorization.Demo.UnitTests` tests the security model of the invoicing application.

Its core is a permission matrix. Each row is an operation of the application with the set of users allowed to perform it:

```csharp
Op( "Approve a sales invoice", org => org.SalesInvoice.Approve(), Mia ),
Op( "Read a customer tax ID", org => _ = org.Fabrikam.TaxId, Mia, Ada, Otto, Grace ),
```

The `PermissionMatrixTests` theory runs every operation as every user, on a fresh organization. The operation must succeed for the users of the row and throw `AccessDeniedException` for all the others. Each row therefore checks both the grants and the denials, and adding a row is enough to cover a new operation. The rows describe the intended business rules, independently of how `DemoPolicy` implements them. The matrix found a gap during the development of this example: an auditor who was also a department manager could create invoices.

The other test classes cover what a single operation cannot show:

* `WorkflowTests`: the invoice lifecycle, role changes that take effect immediately, transfers that change who inherits access, and the difference between a denied permission and a broken business rule.
* `OptionsBehaviorTests`: proof at run time that each build-time option took effect.
* `AuditTests`: the content of audit records for the demo operations.

## Limitations

* The policy evaluates the rules on each access and does not cache decisions. Each check allocates a `Permission` and walks the type hierarchy and the role assignments.
* Rules apply to types, not to object states. "Only draft invoices can be edited" is a business rule in the `Invoice` methods.
* A department manager can assign any role in their department, including roles with more rights. A real system restricts which roles each role may assign.
* Data is not persisted.
