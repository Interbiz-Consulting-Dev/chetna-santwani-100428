# Phase 1 — Foundational domain model

**Status:** complete  
**How to run:** `dotnet run` from `EmployeeManagementSystem`

Core nouns of a small people-ops system: who someone is, where they live, how they are contracted, which department they sit in, and the company rules that apply to every hire.

---

## Use cases

| Use case | Behaviour |
|---|---|
| Hire staff | Allocates a unique staff id, checks salary floor, records contract/role/address |
| Statutory on file | FTE/intern: PAN + UAN. Contractor: GSTIN (shape differs by contract) |
| Assign to a department | Sets `DepartmentId`, increments that department’s headcount |
| Employment summary | Active, on probation, or resigned — driven by optional dates |
| Preview then confirm a move | Proposed `Address` is a copy; on-file address changes only on confirm |
| Resign | Sets last day, status, and drops **active** headcount |
| Org facts | HQ, probation length, salary floor — not stored on a person |

---

## What was modelled

| Type | Kind | Why |
|---|---|---|
| `PersonName` | `readonly struct` | A name is values, not an identity |
| `Address` | `readonly struct` | Same; copy-on-assign makes address drafts safe |
| `Employee` | `class` | A person has identity; many parts of the app share one record |
| `Department` | `class` | A team has identity and its own headcount |
| `CompanyRules` | `static` class | Policy is organisation-wide |
| `EmployeeRole`, `EmploymentStatus`, `ContractType` | `enum` | Small closed sets |

Placeholders for later: `Services/`, `Repositories/`, `Exceptions/`, `Utils/`.

---

## Required concepts — where they landed

| Concept | Placement | Why here |
|---|---|---|
| Value type | `Address`, `PersonName` | Copy semantics; no independent identity |
| Reference type | `Employee`, `Department` | Shared identity |
| Nullable | `LastDay`, `ProbationEndsOn`, `DepartmentId`, `PersonName.Middle` | Genuinely absent for some people/contracts |
| `dynamic` | `Employee.Statutory` | Vendor/statutory fields differ (PAN/UAN vs GSTIN). Tradeoff: no compile-time checks; `Dictionary<string, object>` or typed profiles later |
| `var` | `Program.cs` | Type is obvious from `new` / method return |
| Static | `Employee.AllocateStaffId`, `Employee.ActiveHeadcount`, `CompanyRules` | Ids, active workforce, and policy cannot live on one instance |
| Instance | salary, address, status, `ProposeRelocation`, `AssignTo`, `Resign` | Per person / per department |

No isolated “concept demo” methods. Boxing was not required and has no natural home here, so it is omitted.

---

## What we will do next

Still not storage, files, menus, or threading. Later phases can add role-specific behaviour, a repository, and a console workflow on top of these types.
