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

### Layer 1: the abstract authorization layer

The first layer is abstract. It does not assume any security model. It defines five concepts:

| Concept | Type | Description |
|---|---|---|
| Subject | `ISubject` | The user or service on whose behalf the code runs. |
| Permission | `Permission` | Something that the subject must have on an object, for instance `Read`, `Write` or `Approve`. Permissions are compared by name. |
| Securable | any object | The object on which the permission is required. For a method or property, it is the current instance. For a parameter, it is the argument. |
| Policy | `IAuthorizationPolicy` | Decides whether a subject has a permission on a securable. |
| Security context | `ISecurityContext` | The current subject, the policy, and an optional auditor. `SecurityContext.Current` exposes it. |

The aspects, the options and the run-time entry point `AccessGuard` belong to this layer. They only ask one question: does the current subject have this permission on this object? The answer comes from the `IAuthorizationPolicy.IsGranted` method, and the abstract layer does not know how the policy computes it.

### Layer 2: the role-based model

The second layer is one implementation of `IAuthorizationPolicy`: the classic role-based access control model, in the `RoleBased` namespace.

* Roles are assigned to subjects on an object, for instance "Mia is department manager of Sales".
* Each object has a security parent and inherits the role assignments of its ancestors. For instance, an invoice inherits the role assignments of its department, and the department inherits those of the company.
* Rules allow or deny a permission to a role on a type of object, for instance "department managers are allowed to approve invoices". A rule on a base type also applies to derived types.
* A permission is granted when at least one rule allows it and no rule denies it.
* Two roles are built in and never assigned. Every subject has `Everyone`. The `Owner` of an object has `Owner` on this object only.

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

* There is no check in a system scope, opened with `SecurityContext.RunAsSystem()`. Trusted code uses it, for instance to seed a database or to build an object after its permission has been checked.
* Without a current context, the access fails. A missing context is a configuration error, not a reason to skip security.
* The policy runs in a system scope. The policy reads secured members, such as the owner of the object, and these reads must not trigger checks recursively. The exception message is built in the same scope, because it calls `ToString`.
* On a denial, the auditor receives the exception before it is thrown.

`SecurityContext` stores the context and the system scope in `AsyncLocal<T>` fields. They follow `async` code and stay isolated between concurrent requests.

[!metalama-file Metalama.Samples.Authorization/Framework/SecurityContext.cs]

## The role-based policy

`RoleBasedAuthorizationPolicy` stores the rules by type. `IsGranted` walks the type hierarchy of the securable. For each rule that matches the permission and a role of the subject, a `Deny` rule returns `false` immediately, and an `Allow` rule records the grant.

[!metalama-file Metalama.Samples.Authorization/RoleBased/RoleBasedAuthorizationPolicy.cs]

`RoleBasedSecurable` resolves roles. `Everyone` matches every subject. `Owner` matches the owner of the object itself, and children do not inherit it. Assigned roles are searched on the object and on its security ancestors.

`RoleBasedSecurable` does not store anything. The `Owner` property is abstract, and derived classes store the role assignments through the abstract `GetRoleAssignments`, `AddRoleAssignment` and `RemoveRoleAssignment` methods. `InMemoryRoleBasedSecurable` stores them in a list. The demo stores them in the database. The `[RequiresPermission]` aspect on the abstract `Owner` property is inherited, so every implementation of `Owner` is secured.

[!metalama-file Metalama.Samples.Authorization/RoleBased/RoleBasedSecurable.cs]

## The demo: invoicing at Contoso

The company Contoso has three departments: Sales, Marketing and Engineering. Departments manage customers, issue invoices and have a yearly budget. An invoice moves through the statuses Draft, Approved, Sent, Paid and Voided.

### The security model from the business point of view

This section describes who may do what, in the words of the business, without reference to the code. A reviewer can compare it with the policy and the tests below. The identifiers, such as `INV-4`, also appear in the permission matrix of the tests.

#### Positions

| Position | Works for | Job |
|---|---|---|
| Department manager | one or more departments | Runs the department: approves its invoices, decides the commercial terms of its customers, follows its budget, and staffs it. |
| Department assistant | one department | Prepares and sends the invoices of the department, and keeps the customer contact details up to date. |
| Accountant | the whole company | Collects payments, cancels invoices, and manages the budgets. |
| Auditor | the whole company | Checks the books. Sees everything and changes nothing. |
| Administrator | the whole company | Manages user accounts and access rights. Is not involved in the business. |

The person who creates an invoice is its owner. A person can hold several positions, for instance an auditor who also manages a department.

#### Company directory

* `DIR-1`: Every user, even without any position, can see the name of the company, the names of the departments, and who holds which position in each department.
* `DIR-2`: Nobody can rename a department in the application.

#### Invoices

* `INV-1`: The invoice number is not confidential. Every user can see it.
* `INV-2`: The content of an invoice is visible to the staff of its department, to its owner, to accountants and to auditors. Nobody else can see it, administrators included.
* `INV-3`: The staff of a department can create invoices for the department, only for a customer whose record they can see (`CUS-1`). For instance, a person who works in Sales and in Marketing can invoice a Sales customer from Marketing.
* `INV-4`: Before approval, the owner and the staff of the department can edit the invoice. Accountants cannot edit invoice content.
* `INV-5`: Only a manager of the department can approve an invoice. Nobody can approve an invoice they created (four-eyes principle). An auditor cannot approve, even as a manager.
* `INV-6`: The staff of the department send the approved invoices to the customer.
* `INV-7`: Only accountants record payments.
* `INV-8`: Only accountants cancel (void) invoices.
* `INV-9`: A manager of the department can give the invoice to another owner.
* `INV-10`: A manager can move an invoice from one department to another only when they manage both departments.

#### Customers

* `CUS-1`: A customer is managed by one department. Its record is visible to the staff of the department, to accountants and to auditors.
* `CUS-2`: The staff of the department can change the contact details of the customer.
* `CUS-3`: The tax ID of a customer is confidential. Only the managers of the department, accountants and auditors can see it. Administrators never see it.
* `CUS-4`: Only a manager of the department can change the commercial terms of a customer: credit limit and payment terms.

#### Budgets

* `BUD-1`: Budgets are confidential. The budget of a department is visible to its managers, to accountants and to auditors. Department assistants cannot see it.
* `BUD-2`: Only accountants change budgets and record spending.

#### Access rights

* `ACC-1`: Administrators create user accounts, and assign and revoke positions everywhere.
* `ACC-2`: A department manager assigns and revokes positions in the departments they manage, and nowhere else.
* `ACC-3`: A change of position takes effect immediately, for every user.

#### General principles

* `GEN-1`: Separation of duties: an auditor never changes anything, whatever other position they hold.
* `GEN-2`: Least privilege: administrators manage access, but do not see business data.
* `GEN-3`: Every refused operation is recorded in a security log.
* `GEN-4`: Some operations are refused for business reasons, not for security reasons. For instance, an approved invoice cannot be edited, an invoice without lines cannot be approved, and a paid invoice cannot be voided. These refusals are not security events.

#### Questions for the reviewer

The current model has the following properties. They are deliberate in this example, but a real business may decide otherwise:

* A department assistant can overwrite the tax ID of a customer, although they cannot see it (`CUS-2`, `CUS-3`).
* A department manager can assign any position in their department, including a position with more rights than their own, such as accountant (`ACC-2`). A real system restricts which positions each position may assign.
* The owner of an invoice keeps access to it after the invoice moves to another department (`INV-2`, `INV-10`).
* Every user can see who holds which position (`DIR-1`).
* The rules do not depend on the status of an object. For instance, a manager can change the credit limit of a customer at any time.
* The rules protect the content of the objects, not their existence. A user can search for invoices that they are not allowed to read; the search returns the invoices, but their content stays protected.

### How the demo implements the model

The positions are roles assigned on the company or on a department, and inherited by the objects below. The owner and the "every user" cases are the built-in roles `Owner` and `Everyone`:

| Role | Scope | Responsibilities |
|---|---|---|
| Department manager | department | Approves the invoices of the department, maintains customers and their credit terms, sees sensitive customer data and the department budget, moves invoices between departments they manage, assigns roles in the department. |
| Department assistant | department | Prepares and sends invoices, maintains customer contact details. |
| Accountant | company | Records payments, voids invoices, manages budgets. Does not edit invoice content. |
| Auditor | company | Reads everything, including sensitive data. Every change is denied. |
| Administrator | company | Assigns and revokes roles everywhere. Sensitive data is denied. |
| Owner (built in) | object | Reads and edits the invoice they created, but cannot approve it. |
| Everyone (built in) | all | Reads the company directory. |

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

* Four-eyes principle: the owner of an invoice cannot approve it, even as a department manager.
* Segregation of duties: an auditor cannot change anything, even with a manager role in a department.
* Least privilege: administrators manage access but cannot read sensitive data.

[!metalama-file Metalama.Samples.Authorization.Demo/DemoPolicy.cs]

Authorization is a different concern from business rules. The policy answers "may this user do this?". The `Invoice` methods enforce rules such as "an approved invoice cannot be edited", and they throw `InvalidOperationException`, not `AccessDeniedException`.

Moving an invoice to another department needs a parameter check. `Invoice.TransferTo` requires `ChangeOwnership` on the invoice and on the destination department. Without the second check, a user could move an invoice into a department where they have more rights.

The program runs each scenario as a separate request of a user, through the application services, and prints the outcome. The auditor writes the denials to the log. Here is an excerpt:

```text
Approving: the four-eyes principle and segregation of duties
  GOOD Mia approves Sam's invoice: succeeded
  GOOD Mia adds a line to the approved invoice (business rule): failed: Invoice SAL-0001 is Approved, expected Draft.
warn: Metalama.Samples.Authorization.Demo.Services.LoggingAccessAuditor[277457468] Mia was denied Approve on Invoice.Approve of 'invoice MKT-0001'.
  GOOD Mia approves her own marketing invoice: was denied: Cannot execute Invoice.Approve: 'Mia' does not have the Approve permission on 'invoice MKT-0001'.
warn: Metalama.Samples.Authorization.Demo.Services.LoggingAccessAuditor[277457468] Grace was denied Approve on Invoice.Approve of 'invoice MKT-0001'.
  GOOD Grace, marketing manager but also auditor, approves the marketing invoice: was denied: Cannot execute Invoice.Approve: 'Grace' does not have the Approve permission on 'invoice MKT-0001'.
...
Moving an invoice: checking the parameter
  GOOD Mia creates a sales invoice: succeeded
  GOOD Mia moves it to Marketing, which she also manages: succeeded
warn: Metalama.Samples.Authorization.Demo.Services.LoggingAccessAuditor[277457468] Mia was denied ChangeOwnership on Invoice.TransferTo(target) of 'Engineering'.
  GOOD Mia moves it to Engineering: was denied: Cannot execute Invoice.TransferTo(target): 'Mia' does not have the ChangeOwnership permission on 'Engineering'.
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
* The constructor that Entity Framework uses must not call a secured setter. Each entity has a private constructor for Entity Framework that only sets fields, or that chains to a primary constructor whose parameters initialize the properties. A property initializer sets the backing field and runs no check.
* The storage members of `Entity`, which hold the owner and the role assignments, are excluded from default securing with `[AuthorizationOptions( IsDefaultSecuringEnabled = false )]`. The public `Owner` and `RoleAssignments` members are the secured views of these members, and the model ignores them.
* The navigations that the security model needs are loaded automatically with `AutoInclude`: the parent chain of each object, its owner, and its role assignments. Otherwise, a role inherited from a parent that is not loaded would be missed. The policy would then deny the access, which is safe but wrong.
* By convention, Entity Framework maps only the properties that have a setter. The get-only properties that expose the state of an object, such as `Invoice.Status`, are mapped explicitly. A test checks that every field of every entity is mapped.

[!metalama-file Metalama.Samples.Authorization.Demo/Data/InvoicingDbContext.cs]

A connection to an in-memory SQLite database cannot be used by concurrent requests. `InMemoryDatabase` keeps a named, shared-cache in-memory database open, and each database context opens its own connection to it.

## Tests

### Aspect tests

The aspect tests in `Metalama.Samples.Authorization.Tests` verify the generated code. They cover each kind of target (methods, `async` methods, properties, `init` accessors, fields, method and constructor parameters), default permissions, inheritance by overrides and by abstract members, each level of options, and the eligibility errors.

Two tests, `Demo_SalesConventions` and `Demo_FinanceConventions`, compile the real configuration of the demo application with `// @Include` directives: the `Entity` base class and its storage types, the project fabric, the namespace fabric and the permission names. They show the code that this configuration generates. For instance, in the `Finance` namespace:

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
Op( "INV-5: Approve a sales invoice", org => org.SalesInvoice.Approve(), Mia ),
Op( "CUS-3: Read a customer tax ID", org => _ = org.Fabrikam.TaxId, Mia, Ada, Otto, Grace ),
```

Each row starts with the identifier of the business requirement that it checks (see [The security model from the business point of view](#the-security-model-from-the-business-point-of-view)), so the name of each test case gives the requirement, the operation, the user and the expected outcome.

The `PermissionMatrixTests` theory runs every operation as every user, on a fresh database. The objects are loaded from the database before the operation. The operation must succeed for the users of the row and throw `AccessDeniedException` for all the others. After a successful operation, the test saves the changes as the user, which must not trigger any check. Each row therefore checks both the grants and the denials, and adding a row is enough to cover a new operation. The rows describe the intended business rules, independently of how `DemoPolicy` implements them. The matrix found a gap during the development of this example: an auditor who was also a department manager could create invoices.

The other test classes cover what a single operation cannot show:

* `WorkflowTests`: the invoice lifecycle, role changes that take effect immediately, transfers that change who inherits access, and the difference between a denied permission and a broken business rule.
* `OptionsBehaviorTests`: proof at run time that each build-time option took effect.
* `AuditTests`: the content of audit records for the demo operations.
* `PersistenceTests`: loading and saving run no check; every field of every entity is mapped; authorized changes and role assignments are persisted and effective in a new database context; denied changes leave the database unchanged; queries are not filtered; and complete workflows run through the services as separate requests, including concurrent requests of different users.

## Limitations

* The policy evaluates the rules on each access and does not cache decisions. Each check allocates a `Permission` and walks the type hierarchy and the role assignments.
* Rules apply to types, not to object states. "Only draft invoices can be edited" is a business rule in the `Invoice` methods.
* A department manager can assign any role in their department, including roles with more rights. A real system restricts which roles each role may assign.
* The aspects protect members, not rows. A user without any role can query invoices, but cannot read their members. To filter rows, use another mechanism, for instance an Entity Framework global query filter.
* The demo uses an in-memory database, created and seeded at startup.
