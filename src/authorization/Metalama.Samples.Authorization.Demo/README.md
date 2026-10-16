---
uid: sample-authorization-security-model
level: 300
summary: "The security model of the invoicing demo of the authorization example, in the words of the business: positions, and the rules on invoices, customers, budgets and access rights."
keywords: "authorization, security model, business rules, permissions, roles, four-eyes principle, separation of duties"
created-date: 2026-10-16
modified-date: 2026-10-16
---

# Authorization example: the security model of the demo


This article describes who may do what in the demo application of the [authorization example](../README.md), in the words of the business, without reference to the code. A reviewer can compare it with the policy and the tests that the [main article](../README.md#how-the-demo-implements-the-model) describes. The identifiers, such as `INV-4`, also appear in the permission matrix of the tests.

## Positions

| Position | Works for | Job |
|---|---|---|
| Department manager | one or more departments | Runs the department: approves its invoices, decides the commercial terms of its customers, follows its budget, and staffs it. |
| Department assistant | one department | Prepares and sends the invoices of the department, and keeps the customer contact details up to date. |
| Accountant | the whole company | Collects payments, cancels invoices, and manages the budgets. |
| Auditor | the whole company | Checks the books. Sees everything and changes nothing. |
| Administrator | the whole company | Manages user accounts and access rights. Is not involved in the business. |

The person who creates an invoice is its owner. A person can hold several positions, for instance an auditor who also manages a department.

## Company directory

* `DIR-1`: Every user, even without any position, can see the name of the company, the names of the departments, and who holds which position in each department.
* `DIR-2`: Nobody can rename the company or a department in the application.

## Invoices

* `INV-1`: The invoice number is not confidential. Every user can see it.
* `INV-2`: The content of an invoice is visible to the staff of its department, to its owner, to accountants and to auditors. Nobody else can see it, administrators included.
* `INV-3`: The staff of a department can create invoices for the department, only for a customer whose record they can see (`CUS-1`). For instance, a person who works in Sales and in Marketing can invoice a Sales customer from Marketing.
* `INV-4`: Before approval, the owner and the staff of the department can edit the invoice, including the customer that it bills. The new customer must be one whose record they can see. Accountants cannot edit invoice content.
* `INV-5`: Only a manager of the department can approve an invoice. Nobody can approve an invoice that they created or that they own (four-eyes principle), even after giving the invoice to another owner. An auditor cannot approve, even as a manager.
* `INV-6`: The staff of the department send the approved invoices to the customer.
* `INV-7`: Only accountants record payments.
* `INV-8`: Only accountants cancel (void) invoices.
* `INV-9`: A manager of the department can give the invoice to another owner.
* `INV-10`: A manager can move an invoice from one department to another only when they manage both departments.
* `INV-11`: An invoice has internal notes. Everyone who can see the invoice can read them. The owner and the staff of the department can add or remove notes, at any status.

## Customers

* `CUS-1`: A customer is managed by one department. Its record is visible to the staff of the department, to accountants and to auditors.
* `CUS-2`: The staff of the department can change the contact details of the customer.
* `CUS-3`: The tax ID of a customer is confidential. Only the managers of the department, accountants and auditors can see it. Administrators never see it.
* `CUS-4`: Only a manager of the department can change the commercial terms of a customer: credit limit and payment terms.
* `CUS-5`: A manager can move a customer from one department to another only when they manage both departments.

## Budgets

* `BUD-1`: Budgets are confidential. The budget of a department is visible to its managers, to accountants and to auditors. Department assistants cannot see it.
* `BUD-2`: Only accountants change budgets and record spending.
* `BUD-3`: The staff of a department, assistants included, can see whether the budget of their department is available, low or exhausted, without seeing its amounts.

## Access rights

* `ACC-1`: Administrators create user accounts, and assign and revoke positions everywhere.
* `ACC-2`: A department manager assigns and revokes positions in the departments they manage, and nowhere else.
* `ACC-3`: A change of position takes effect immediately, for every user.
* `ACC-4`: Nobody can give themself a position, administrators included.

## General principles

* `GEN-1`: Separation of duties: an auditor never changes anything, whatever other position they hold.
* `GEN-2`: Least privilege: administrators manage access, but do not see business data.
* `GEN-3`: Every refused operation is recorded in a security log, and so is every change of positions or of ownership. A refusal message names the object only by its type and number, so that it discloses nothing to a user who cannot see the object.
* `GEN-4`: Some operations are refused for business reasons, not for security reasons. For instance, an approved invoice cannot be edited, an invoice without lines cannot be approved, and a paid invoice cannot be voided. These refusals are not security events.

## Questions for the reviewer

The current model has the following properties. They are deliberate in this example, but a real business may decide otherwise:

* A department assistant can overwrite the tax ID of a customer, although they cannot see it (`CUS-2`, `CUS-3`).
* The status of a budget discloses a part of its amounts: a department assistant knows when less than a tenth of the budget remains (`BUD-1`, `BUD-3`).
* A department manager can assign any position in their department to another person, including a position with more rights than their own, such as accountant (`ACC-2`). They cannot take it themself (`ACC-4`), but a real system also restricts which positions each position may assign.
* The owner of an invoice keeps access to it after the invoice moves to another department (`INV-2`, `INV-10`).
* Every user can see who holds which position (`DIR-1`).
* The rules do not depend on the status of an object. For instance, a manager can change the credit limit of a customer at any time.
* The rules protect the content of the objects, not their existence. A user can search for invoices that they are not allowed to read; the search returns the invoices, but their content stays protected.
