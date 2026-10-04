# Employee Management System — Complete Deep Dive

## What Is This App?

A **C# console application** that simulates a company's HR department. It lets you hire, manage, pay, and offboard employees, with all data persisted to a plain-text file (`workforce.txt`). It is a teaching project that deliberately showcases every core OOP and C# concept: inheritance, polymorphism, encapsulation, events, async/await, threading, generics, and file I/O.

---

## Architecture at a Glance

```
┌──────────────────────────────────────────────────────────────┐
│                        Program.cs                            │
│  (Entry point: wire up everything, then hand off to menu)    │
└──────┬───────────┬──────────────┬─────────────┬─────────────┘
       │           │              │             │
       ▼           ▼              ▼             ▼
EmployeeFile  EmployeeRepo  EmployeeNotif  EmployeeHeadcount
Storage       (in-memory    Service        Monitor
(disk I/O)    data store)   (event hooks)  (background thread)
       │           │
       │    ┌──────┴──────────────────────────┐
       │    │         Models                  │
       │    │  Employee (abstract)             │
       │    │  ├── Developer                  │
       │    │  ├── Manager (+ Manager.Team)   │
       │    │  ├── Intern                     │
       │    │  └── Contractor                 │
       │    └─────────────────────────────────┘
       │
EmployeeConsoleMenu
(all 9 menu operations)
```

---

## Layer-by-Layer Breakdown

### 1. Entry Point — [`Program.cs`](./Program.cs)

The startup sequence in order:

| Step | What happens |
|------|-------------|
| 1 | Determine file paths: `data/workforce.txt` and `data/errors.log` |
| 2 | Create `EmployeeFileStorage` — acquires an **exclusive file lock** (`.lock` file) so two instances can't corrupt the same roster |
| 3 | `await fileStorage.LoadAsync()` — reads and deserialises the flat-file roster from disk |
| 4 | `EnsureDefaultDepartments()` — seeds Engineering (id=10) and People Ops (id=20) if they don't exist |
| 5 | Wire up `EmployeeNotificationService` — subscribes to all events on the repository |
| 6 | Start `EmployeeHeadcountMonitor` — launches a background thread that polls `Employee.ActiveHeadcount` every 1 second |
| 7 | Run `EmployeeConsoleMenu.RunAsync()` — blocks here until the user exits |
| 8 | On exit: cancel the background thread, join it, then `SaveAsync()` the full roster back to disk |

> **Key design**: The `using` keyword on `EmployeeFileStorage` guarantees the OS lock file is released even if an exception is thrown (implements `IDisposable`).

---

### 2. Models — The Inheritance Hierarchy

#### [`Employee.cs`](./Models/Employee.cs) — Abstract Base Class

The core of the whole system. Everything that belongs to every person on the books lives here.

**Static state (process-wide):**
| Field | Purpose |
|-------|---------|
| `_nextStaffId` | Auto-incrementing counter; each `new Employee()` call claims the next id |
| `_activeHeadcount` | Live count of non-resigned/terminated people — read by the background monitor |

**Key methods:**

- `SetMonthlySalary(decimal)` — **virtual, protected** — validates salary ≥ floor and that the employee hasn't already left. All salary changes route through here.
- `ComputeVariablePay()` — **abstract** — each subtype has its own bonus formula.
- `NoticePeriodDays` — **abstract property** — each subtype declares its own notice period.
- `Resign(DateTime)` — marks status as `Resigned`, records the last day, decrements `_activeHeadcount`.
- `AssignTo(Department)` — links the employee to a department, also calls `department.Register(this)`.
- `ProposeRelocation()` / `ConfirmRelocation()` — two-step address change (preview before committing).
- `ResetCensusForFileLoad()` — **internal static** — rewinds both static counters before loading from disk, so IDs don't double-count.

**Two constructors:**
1. **Hire constructor** — for new employees: allocates a fresh id, increments headcount.
2. **Restore constructor** — for loading from disk (`EmployeeState`): restores the saved id, does not re-allocate.

---

#### Concrete Employee Types

| Class | Notice | Variable Pay Formula | Special Rule |
|-------|--------|---------------------|--------------|
| [`Developer`](./Models/Developer.cs) | 30 days | 8% of base (10% if skill contains "C#") | Has `PrimarySkill` property |
| [`Manager`](./Models/Manager.cs) | 90 days | 3% × base × number of direct reports | Implements `ITeamCapable`; split into `Manager.cs` + `Manager.Team.cs` |
| [`Intern`](./Models/Intern.cs) | 7 days | ₹0 (no bonus) | Overrides `SetMonthlySalary` to cap stipend at ₹25,000 |
| [`Contractor`](./Models/Contractor.cs) | 15 days | 5% retainer (not a bonus, not on FTE plan) | Uses GSTIN (not PAN/UAN); no probation period |

**Polymorphism in action**: Payroll (`RunPayroll` menu) just calls `employee.ComputeVariablePay()` on each person — it doesn't know or care which subtype it's dealing with.

---

#### Value Objects

| Class | What it represents |
|-------|--------------------|
| [`PersonName`](./Models/PersonName.cs) | First, Last, optional Middle name |
| [`Address`](./Models/Address.cs) | Street, City, State, PostalCode + `RelocateTo()` returns a new Address (immutable-style) |
| [`Department`](./Models/Department.cs) | Id, Name, headcount tracking, and `Register(Employee)` |
| [`EmployeeState`](./Models/EmployeeState.cs) | A flat DTO (Data Transfer Object) used only during file load/save; not a live domain object |

---

### 3. Repository — [`EmployeeRepository.cs`](./Repositories/EmployeeRepository.cs)

The **single in-memory source of truth**. It holds four internal data structures for different access patterns:

| Data Structure | Type | Access Pattern |
|---------------|------|---------------|
| `_hireOrder` | `List<Employee>` | Ordered iteration (roster, payroll walks) |
| `_byStaffId` | `Dictionary<int, Employee>` | O(1) lookup by badge/ID |
| `_departments` | `Dictionary<int, Department>` | Department queries |
| `_byDepartment` | `Dictionary<int, List<Employee>>` | "Who is in Engineering?" without scanning everyone |
| `_legacyBadgeToStaffId` | `Hashtable` | Old turnstile tokens → staff ids (non-generic, by spec) |
| `_legacyPayrollIdBatch` | `ArrayList` | Legacy payroll system drop (non-generic, by spec) |

> **Why two non-generic collections?** The spec explicitly requires demonstrating `System.Collections` (pre-generics) types. In real code you'd use `Dictionary<string, int>` and `List<int>`.

**Events published** (Observer pattern):

| Event | Delegate type | Fired when |
|-------|--------------|-----------|
| `StaffHired` | `StaffLifecycleHandler` | `Hire()` is called with `announce=true` |
| `StaffLeft` | `StaffLifecycleHandler` | `RecordResignation()` |
| `StaffAssigned` | `StaffAssignedHandler` | `AssignToDepartment()` with `announce=true` |
| `CompensationChanged` | `CompensationChangedHandler` | `RecordRaise()` |

---

### 4. Notifications — [`EmployeeNotificationService.cs`](./Notifications/EmployeeNotificationService.cs)

Subscribes to all four repository events and reacts without the repository knowing:

```
StaffHired  →  WriteHireAudit (console print)
            →  CountHire (session stats)
            →  lambda: probation watch if new hire is on probation

StaffAssigned → lambda: console print assignment

CompensationChanged → WriteRaiseAudit (console + session counter)
                    → MarkPayrollDirty (flag for future payroll systems)

StaffLeft   → WriteExitAudit (console print)
            → CountExit (session stats)
```

**Multicast delegates** — one event can have multiple handlers. Both `WriteHireAudit` AND `CountHire` fire for the same `StaffHired` event.

---

### 5. Background Thread — [`EmployeeHeadcountMonitor.cs`](./Services/EmployeeHeadcountMonitor.cs)

A dedicated background thread that polls `Employee.ActiveHeadcount` every 1 second.

- Uses `Volatile.Read` and `Interlocked.Exchange` for **thread-safe reads** of the static counter without a lock.
- Respects `CancellationToken` — exits the loop when the main thread cancels.
- `IsBackground = true` — won't prevent the process from exiting if something goes wrong.
- `Join()` — the main thread waits for this thread to finish before printing the final sample count.

---

### 6. Persistence — [`EmployeeRosterStore.cs`](./Persistence/EmployeeRosterStore.cs)

Serialises/deserialises the roster to/from a **tab-delimited text file** (`workforce.txt`).

**File format:**
```
# Northwind roster v1
DEPT    10    Engineering
DEPT    20    People Ops
PERSON  Developer  1  Alice    M  Smith  Developer  FullTime  Active  2024-01-15  80000  Performance review  ...
PERSON  Manager    2  Bob      Kumar  Manager  FullTime  Active  2024-02-01  120000  none  ...
BADGE   STR  ALICE-BADGE-42  1
```

**Save sequence (atomic write):**
1. Build the full text in memory via `BuildRosterText()`
2. Write to a **temp file** (`workforce.txt.tmp`) asynchronously
3. On success: `File.Copy(temp → actual)` then `File.Delete(temp)`
4. On cancel/error: `TryDelete(temp)` to clean up

This means the actual file is never left in a half-written state.

**Load sequence:**
1. `Employee.ResetCensusForFileLoad()` — resets static counters
2. Read all lines from disk
3. Parse each line: `DEPT` lines registered immediately; `PERSON`/`BADGE` lines buffered into `pendingPeople`
4. Second pass: `AttachPerson()` — create the right subtype, hire it (silently, no events), assign department if saved

**Statutory encoding per employee type:**
- FTE (Developer/Manager/Intern): `PAN=ABCDE1234F;UAN=100123456789`
- Contractor: `GSTIN=27AAAPA0000A1Z5`

**Extra field:**
- Developer: stores `PrimarySkill`
- Manager: stores semicolon-separated direct report names

---

### 7. Services

#### [`EmployeeConsoleMenu.cs`](./Services/EmployeeConsoleMenu.cs) — The Interactive UI

The main event loop (`RunAsync`) shows 9 options:

| Option | What it does |
|--------|-------------|
| 1. Hire | Prompts for type, name, address, date, salary, PAN/UAN or GSTIN; creates the right subtype; optionally assigns a department |
| 2. View roster | Filter by: everyone / active / leavers / by department |
| 3. Assign department | Pick employee + department |
| 4. Give raise | Pick employee, enter amount + reason |
| 5. Address move | Two-step: propose → confirm or discard |
| 6. Resign | Pick employee, enter last day |
| 7. Run payroll | List all active employees with base + variable pay; print totals |
| 8. Recent errors | Reads last 5 entries from `errors.log` |
| 9. Save now | Manual mid-session save |
| 0. Exit | Saves and quits |

All input helpers (`ReadInt`, `ReadDecimal`, `ReadDate`, etc.) loop until valid input is given — no crash on bad input.

#### [`ErrorLog.cs`](./Services/ErrorLog.cs) — Async-Safe Error Logging

Uses a `SemaphoreSlim(1,1)` as an **async mutex** so concurrent async operations don't interleave writes. Each log entry is delimited by `-----`. Provides both sync (`Record`) and async (`RecordAsync`) variants.

---

### 8. Policies — [`CompanyRules.cs`](./Policies/CompanyRules.cs)

All company-wide constants in one place, as a `static class`:

| Rule | Value |
|------|-------|
| Minimum monthly salary | ₹15,000 |
| Maximum intern stipend | ₹25,000 |
| Probation period | 90 days (from hire date) |
| Contractors | No probation (they're on a term, not a probation cycle) |
| HQ address | 14 Embassy Tech Village, Bengaluru, KA 560103 |

---

### 9. Enums — [`Enums/`](./Enums/)

| Enum | Values |
|------|--------|
| `EmployeeRole` | Developer, Manager, TeamLead, Intern, HR |
| `ContractType` | FullTime, Contractor, Intern |
| `EmploymentStatus` | Active, Resigned, Terminated |

---

### 10. Exceptions — [`EmsExceptions.cs`](./Exceptions/EmsExceptions.cs)

Custom exception hierarchy:

```
Exception
└── EmsException                    (base for all domain errors)
    ├── SalaryFloorViolationException   (salary < ₹15,000)
    ├── InternStipendLimitException     (stipend > ₹25,000)
    ├── InvalidRaiseException           (raise amount ≤ 0)
    ├── EmploymentEndedException        (salary change after exit)
    ├── AlreadyExitedException          (resign after already exited)
    └── RosterFormatException           (bad line in workforce.txt)
```

The menu catches `EmsException` (and `ArgumentException`, `InvalidOperationException`) separately from generic `Exception` so domain errors print a friendly message and get logged, without crashing.

---

## Complete Data Flow: Hiring an Employee

```
User types "1" (Hire)
  └─► EmployeeConsoleMenu.HireEmployee()
        ├─ ReadName() → PersonName("Alice", "Smith")
        ├─ ReadAddress() → Address("12 MG Road", "Bengaluru", "KA", "560001")
        ├─ ReadDate() → DateTime(2026-01-10)
        ├─ ReadDecimal() → 80000m
        ├─ ReadRequired("Primary skill") → "C#"
        ├─ ReadFteStatutory() → { Pan="ABC..", Uan="100..." }
        │
        └─ new Developer(name, address, date, 80000, "C#", statutory)
              │
              └─ Employee constructor (base)
                    ├─ AllocateStaffId() → Id=1
                    ├─ CompanyRules.ComputeProbationEnd() → date+90
                    ├─ SetMonthlySalary(80000)  ← validates ≥ 15000
                    └─ _activeHeadcount++
        │
        └─ _employees.Hire(developer)
              ├─ _hireOrder.Add(developer)
              ├─ _byStaffId[1] = developer
              └─ StaffHired?.Invoke(developer)
                    ├─ WriteHireAudit → console: "[ops] hired Alice [1] Developer"
                    ├─ CountHire → HiresThisSession++
                    └─ lambda: IsOnProbation? → ProbationWatch++, console print
        │
        └─ "Assign a department now? (y/n)"
              └─ _employees.AssignToDepartment(developer, dept10)
                    ├─ developer.AssignTo(dept10) → DepartmentId=10
                    ├─ _byDepartment[10].Add(developer)
                    └─ StaffAssigned?.Invoke → console: "[ops] assigned Alice → Engineering"
```

---

## Complete Data Flow: Saving to Disk

```
User exits (option 0)
  └─► cancellation.Cancel()
        └─ EmployeeHeadcountMonitor exits its while loop, thread ends
  └─► fileStorage.SaveAsync(employees)
        └─ BuildRosterText(employees)
              ├─ WriteDepartments → "DEPT\t10\tEngineering\n"
              ├─ WritePeople     → "PERSON\tDeveloper\t1\tAlice\t\tSmith\t..."
              └─ WriteBadges     → "BADGE\tSTR\tALICE-42\t1"
        └─ WriteAllTextAsync(workforce.txt.tmp)
        └─ File.Copy(tmp → workforce.txt, overwrite=true)
        └─ File.Delete(tmp)
        └─ Returns (2 departments, 1 person, 0 badges)
  └─► fileStorage.Dispose() → releases .lock file
```

---

## Key C# Concepts Demonstrated

| Concept | Where |
|---------|-------|
| **Abstract class** | `Employee` — can't instantiate directly, forces subtypes to implement `ComputeVariablePay` and `NoticePeriodDays` |
| **Inheritance** | `Developer`, `Manager`, `Intern`, `Contractor` all extend `Employee` |
| **Polymorphism** | `RunPayroll` calls `ComputeVariablePay()` without knowing the subtype |
| **Method overriding** | `Intern.SetMonthlySalary` adds a cap; `Manager.FormatNotificationLabel` appends team size |
| **Interface** | `ITeamCapable` — only `Manager` implements it (can lead a team) |
| **Partial class** | `Manager.cs` + `Manager.Team.cs` — separates identity from team management |
| **Events + delegates** | `StaffHired`, `StaffLeft`, `CompensationChanged`, `StaffAssigned` |
| **Multicast delegates** | Multiple handlers on one event |
| **async/await** | File I/O in `LoadAsync`, `SaveAsync`, `RecordAsync` |
| **CancellationToken** | Threaded through all async operations for graceful cancellation |
| **Background thread** | `EmployeeHeadcountMonitor` — `Thread`, `Volatile.Read`, `Interlocked` |
| **IDisposable** | `EmployeeFileStorage` — deterministic release of the OS lock file |
| **SemaphoreSlim** | `ErrorLog` — async-safe mutual exclusion for concurrent log writes |
| **Generics** | `List<Employee>`, `Dictionary<int, Employee>` — type-safe collections |
| **Non-generic collections** | `Hashtable`, `ArrayList` — legacy/spec-required |
| **Value objects** | `PersonName`, `Address` — immutable-style, no identity |
| **Static class** | `CompanyRules` — org-level facts, not instance data |
| **Custom exceptions** | Full hierarchy rooted at `EmsException` |
| **Atomic file write** | Write to `.tmp`, copy over, delete `.tmp` — never half-written |
| **Tuples** | `SummarizeActivePay()` returns `(int People, decimal BasePay, decimal VariablePay)` |
| **Dynamic** | `Statutory` field — PAN/UAN vs GSTIN schemas differ until typed profiles exist |
| **Nullable types** | `LastDay?`, `ProbationEndsOn?`, `DepartmentId?` |
