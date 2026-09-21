# Phase 5 — Delegates, events, lambdas, tuples

**Status:** complete  
**How to run:** `dotnet run` from `EmployeeManagementSystem`

The roster still owns hire / assign / raise / resign. It no longer owns “what happens next.” `PeopleOpsDesk` subscribes; Workforce does not know about audit lines, session counts, or probation watch.

---

## Use cases

| Action | Notification | Why this one |
|---|---|---|
| Hire | `StaffHired` | Onboarding, census, probation watch |
| Assign department | `StaffAssigned` | Org chart / seating |
| Raise | `CompensationChanged` | Audit + payroll must refresh |
| Resign | `StaffLeft` | Offboarding + census |
| Address change | none | Local HR edit; no second system to wake |
| Save/load | no event | Load uses silent hire so we don’t fake 4 new joiners; Save returns a tuple of counts instead |

---

## Concepts

| Concept | Where |
|---|---|
| Delegate | `StaffLifecycleHandler`, `StaffAssignedHandler`, `CompensationChangedHandler` |
| Event / multicast | `StaffHired` → audit **and** census (and a lambda for probation) |
| Lambda | Hire → probation watch; assign → one-line org note |
| Tuple | `SummarizeActivePay()` → `(People, BasePay, VariablePay)`; `Save` → `(Departments, People, Badges)` |
| virtual/override | `FormatNotificationLabel()` — default name/role; Manager adds team size; Intern adds track. Phase 2 already had abstract pay (no default) and virtual `DescribeStatutory`. This virtual has a **default** the desk can call without type switches. |

File load calls `Hire(..., announce: false)` so reconstituting the file is not a burst of new-hire events.

---

## Next

Custom exceptions, then threading, then a menu.
