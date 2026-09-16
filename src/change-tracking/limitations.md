---
uid: sample-dirty-limitations
summary: "Change tracking in the described system only supports immutable fields. Mitigation strategies include manual `OnChange` calls, using immutable collections, or creating change-tracking collection classes."
created-date: 2023-04-18
modified-date: 2024-09-09
---

# Change Tracking example: limitations

A significant limitation of the aspects implemented in the articles above is that they only support fields of _immutable_ type. When a change is made in a child object without changing the field referencing the child object, the <xref:System.ComponentModel.IChangeTracking.IsChanged> property will not be modified. The most common examples are child collections, typically stored in read-only fields or properties.

There are several strategies to mitigate this limitation:

* Manually call `OnChange` from collection operations.
* Use immutable collections.
* Create change-tracking collection classes.

Unless you create change-tracking collection classes, you must design your class to expose the collections as read-only interfaces to prevent the caller code from _skipping_ the call to `OnChange`. You could add code to the `BuildAspect` method to verify that all exposed fields are immutable or implement the change-tracking mechanism.

If you have an object model with parent-child relationships, you may need to call the `OnChange` method of the parent object when any child object is being modified.



