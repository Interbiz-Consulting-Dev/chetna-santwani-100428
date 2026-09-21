# Phase 3 — Collections & in-memory repository

**Status:** complete  
**How to run:** `dotnet run` from `EmployeeManagementSystem`  
**Builds on:** Phase 1 types + Phase 2 employee hierarchy

`Main` no longer keeps the workforce as four local variables. Hires go into `Workforce`, then HR looks people up, lists a department, runs pay, and lists leavers.

---

## Use cases

| Use case | Behaviour |
|---|---|
| Register a department | Engineering and People Ops on the books |
| Hire onto the roster | Add to hire-order list **and** id index |
| Place in a department | Updates the employee and the department index |
| Open a person by staff id | Dictionary lookup |
| Reception badge lookup | Legacy Hashtable (string or int token) |
| List a department | Secondary index, no full-company scan |
| Monthly pay | Walk **active** staff only |
| Address / raise | Same Phase 2 rules, person loaded from the roster |
| Resign + leavers list | Status change stays on the object; roster query finds exits |
| Legacy payroll drop | ArrayList of staff ids for an old vendor file |

---

## Collection choices

| Store | Type | Operation | Why |
|---|---|---|---|
| `_hireOrder` | `List<Employee>` | Directory, payroll walk, active/leaver filters | Ordered, append-only hire sequence |
| `_byStaffId` | `Dictionary<int, Employee>` | `FindByStaffId` | O(1) vs scanning the list |
| `_departments` | `Dictionary<int, Department>` | Register / find a team | Id is the natural key |
| `_byDepartment` | `Dictionary<int, List<Employee>>` | `ListByDepartment` | HR asks “who is in Engineering?” constantly |
| `_legacyBadgeToStaffId` | `Hashtable` | Untyped turnstile tokens | Spec + legacy object keys; I’d use `Dictionary<string,int>` |
| `_legacyPayrollIdBatch` | `ArrayList` | Old payroll id dump | Spec; ints are boxed; I’d use `List<int>` |

Filtering is plain `for` loops — no LINQ.

---

## Boxing/unboxing

`Utils/ConceptNotes.BoxingUnboxingExample()` is **not** a product feature. Isolated and labeled in `Main`. (ArrayList.Add of an `int` also boxes; that is a cost of the legacy API, not a reason to keep the teaching method.)

---

## What `Main` does

1. Open `Workforce`, register two departments.  
2. Hire four people (same Phase 2 types), assign Engineering vs People Ops.  
3. Lookup by id and by badge.  
4. Print each department, then active payroll.  
5. Asha move + raises; Kiran resigns; print leavers.  
6. Legacy payroll id drop.  
7. Boxing concept note.

**Not in this phase:** file persistence, custom exceptions, delegates, threading, menu.

---

## Next

Phase 4 is likely structured error handling (`/Exceptions`). Still no files or menu.
