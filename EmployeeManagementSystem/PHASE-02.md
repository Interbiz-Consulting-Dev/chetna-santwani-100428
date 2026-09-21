# Phase 2 — Object-oriented design

**Status:** complete  
**How to run:** `dotnet run` from `EmployeeManagementSystem`  
**Builds on:** Phase 1 domain (same company rules, addresses, statutory `dynamic`, ids)

Phase 1 stored everyone as one `Employee` plus a role enum. That hid real differences in pay, notice, and team leadership. This phase makes those differences types.

---

## Use cases

| Use case | Behaviour |
|---|---|
| Hire by contract | `Developer` / `Manager` (FTE), `Intern`, `Contractor` — no `new Employee()` |
| Run monthly pay | Same call `ComputeVariablePay()`: FTE bonus, manager scales with reports, intern 0, contractor retainer |
| Lead a team | Only `ITeamCapable` (Manager) can add direct reports |
| Raise | Amount only, or amount + audit reason; rejected if ≤ 0 or person has left |
| Intern stipend cap | Interns cannot go above `CompanyRules.MaximumInternStipendInr` |
| Notice on exit | Each type has its own notice period (shown on resignation summary) |
| Phase 1 still works | Assign department, address preview/confirm, probation, statutory PAN/UAN vs GSTIN, active headcount |

---

## Hierarchy

```
Employee (abstract)          ← IS a person on the books
├── Developer                ← FTE IC; skill-based variable pay; 30-day notice
├── Manager : ITeamCapable   ← FTE lead; pay scales with reports; 90-day notice
├── Intern                   ← stipend, no variable pay, 7-day notice, stipend cap
└── Contractor               ← GSTIN, 5% retainer not FTE bonus, 15-day notice, no probation
```

Split is by **employment relationship**, not job title. Nikhil is still a developer *by role enum* but a `Contractor` by type — same as Phase 1 (Developer + Contractor). Putting him under `Developer` would put him on the FTE bonus plan, which is wrong.

`ITeamCapable` is a **capability**. Team APIs do not belong on `Employee`.

---

## Required concepts

| Concept | Where |
|---|---|
| Inheritance | `Developer` / `Manager` / `Intern` / `Contractor` : `Employee` |
| Encapsulation | `SetMonthlySalary` — floor, intern ceiling, no change after exit |
| Polymorphism | Payroll loop: `Employee` references → `ComputeVariablePay` / `NoticePeriodDays` |
| Overloading | `GiveRaise(amount)` vs `GiveRaise(amount, reason)` |
| Abstract class | `Employee` — identity; must have pay + notice rules |
| Interface | `ITeamCapable` — optional team leadership |
| Partial class | `Manager.cs` + `Manager.Team.cs` — light split (type is not huge yet) |

---

## What `Main` does

One people-ops run: hire four people as `Employee` variables, assign Engineering, Meera takes reports, print roster/statutory/summaries, run pay, confirm Asha’s move, raises (including a rejected zero), Kiran resigns, raise-after-exit rejected.

**Not in this phase:** repository, files, custom exception types, menu, threading.

---

## Next

Phase 3: in-memory collections + a repository so `Main` does not hold one variable per person. Still no LINQ.
