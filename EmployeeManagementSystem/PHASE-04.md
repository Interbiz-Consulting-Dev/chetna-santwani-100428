# Phase 4 — File persistence

**Status:** complete  
**How to run:** `dotnet run` from `EmployeeManagementSystem`  
**Builds on:** Phase 3 `Workforce` + Phase 2 employee subtypes

The roster is written to `data/workforce.txt` (next to the built exe). `Main` still runs people-ops in memory, then saves, then **loads into a new `Workforce`** so a Manager comes back as a Manager.

---

## Use cases

| Use case | Behaviour |
|---|---|
| First run / missing file | Load returns empty books and prints a message |
| Empty file | Same — empty books |
| Save after a working day | Atomic write: temp file, then replace the real file |
| Load | Rebuilds departments, people (correct subtype), badges |
| Malformed line | Skip that line, print a warning, keep the rest |
| Crash mid-write | Main file stays last-good; `.tmp` is discarded |

---

## File format

Tab-separated text (no extra JSON library). Addresses contain commas, so CSV would fight us.

```
# Northwind roster v1
DEPT<TAB>id<TAB>name
PERSON<TAB>Kind<TAB>id<TAB>... <TAB>statutory<TAB>extra
BADGE<TAB>INT|STR<TAB>token<TAB>staffId
```

`Kind` is `Developer` / `Manager` / `Intern` / `Contractor` so load can `new` the right type. Extra holds primary skill (developer) or `;`-joined report names (manager).

---

## Subtype reconstruction

`EmployeeState` + a protected restore constructor (saved id, salary, status, last day) — **not** the hire constructor, which would mint new ids and refuse salary on leavers.

`RosterStore.CreateSubtype` switches on `Kind`.

Census counters are process-wide, so load calls `Employee.ResetCensusForFileLoad()` then rebuilds them from the file.

---

## IDisposable vs finalizer

`RosterStore` implements `IDisposable` because it holds an exclusive `FileStream` lock for the session. Save/load also use `using StreamWriter`.

**No finalizer.** We do not own a raw unmanaged handle. `FileStream` already finalizes if `Dispose` is skipped. A finalizer on `RosterStore` would only wrap a managed object — the wrong pattern.

---

## Next

Custom exceptions (Phase 6 in the original map) or delegates — still no menu or threading.
