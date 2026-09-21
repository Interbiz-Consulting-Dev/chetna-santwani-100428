# Phase 8 — Menu-driven console

**Status:** complete  
**How to run:** `dotnet run` from `EmployeeManagementSystem`

The scripted integration walkthrough has been replaced by an interactive console application.

## Menu options

1. Hire a Developer, Manager, Intern, or Contractor.
2. View everyone, active staff, leavers, or a department.
3. Assign an employee to a registered department.
4. Give a raise with a required reason.
5. Preview and optionally confirm an address move.
6. Record a resignation.
7. Run polymorphic payroll for active staff.
8. View the five most recent structured error-log entries.
9. Save immediately.
0. Save and exit.

Every typed value is validated locally. Domain exceptions from earlier phases are caught at the menu boundary, displayed as user-facing messages, and sent to the existing asynchronous error logger.

## Application lifecycle

`Program` loads the persisted roster before showing the menu and creates the default Engineering and People Ops departments only when their ids are absent. It starts the Phase 7 headcount monitor before entering the menu. On exit, `finally` cancels and joins the monitor, then awaits the async roster save.

The menu is intentionally an interaction layer. It calls `Workforce`, employee domain methods, `RosterStore`, `ErrorLog`, and the existing notification subscriptions instead of duplicating business rules.
