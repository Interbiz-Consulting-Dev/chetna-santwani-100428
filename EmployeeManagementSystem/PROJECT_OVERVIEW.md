# Employee Management System

## Purpose

EMS is a .NET 10 console application for basic employee and payroll management. It demonstrates C# and OOP concepts through working features rather than standalone examples.

## Run the app

From the workspace root (`EMS`):

```powershell
dotnet run --project .\EmployeeManagementSystem\EmployeeManagementSystem.csproj
```

The app starts with three sample employees and three departments. Menu option `7` runs payroll and writes a payroll export under `data` relative to the current working directory.

## What the app does

- Hire a Developer, Manager, Intern, or Contractor.
- Validate names, addresses, statutory IDs, salary, dates, and menu input.
- Display all employees, active employees, resigned employees, or employees assigned to a selected department.
- Assign an employee to a department.
- Apply a positive salary raise with a reason.
- Preview an address change and confirm or cancel it.
- Record an employee resignation and last working day.
- Calculate payroll for active employees and write a legacy-style payroll text export.

## OOP and C# concepts

### Abstraction and inheritance

`Employee` is an abstract base class for `Developer`, `Manager`, `Intern`, and `Contractor`. Shared identity, salary, status, address, and employment behavior live on `Employee`; each concrete type supplies its own pay and notice-period behavior.

### Polymorphism

Payroll calls `ComputeVariablePay()` through `Employee` references. The runtime implementation depends on the concrete type:

- `Developer`: 10% when the primary skill contains `C#`; otherwise 8%.
- `Manager`: 3% of base salary per direct report.
- `Intern`: no variable pay.
- `Contractor`: 5% retainer.

Notice periods and statutory descriptions are also supplied by the concrete employee types.

### Encapsulation

Salary and employment state are controlled by model methods. Salary changes pass through validation for the company minimum, the intern stipend maximum, and whether the employee has left. Raise reasons are recorded with the salary change.

### Value types

`PersonName` and `Address` are immutable `readonly struct` types. Address changes are first created as a proposed value; the menu replaces the stored address only after confirmation.

### Overloading and partial classes

`Employee.GiveRaise` has overloads with and without a reason. `Manager.AddDirectReport` accepts either an `Employee` or a name. The `Manager` class is split across `Manager.cs` and `Manager.Team.cs` with the `partial` keyword.

### Enums and static policy

`EmployeeRole`, `ContractType`, and `EmploymentStatus` represent fixed choices. `CompanyRules` contains shared salary limits and probation calculations. `Employee` allocates staff IDs and tracks active headcount statically for the current process.

### Delegates and lambdas

There is no custom delegate declaration. The app uses lambdas with `List<T>.Find` and `List<T>.FindAll`; those methods accept the built-in `Predicate<T>` delegate. Examples in `EmployeeConsoleMenu` filter employees by status or department and find an employee or department by ID. The project does not use LINQ.

### Boxing and unboxing

`LegacyPayrollExporter` uses `ArrayList` to model an untyped legacy payroll boundary. Adding `int` and `decimal` values to its `object[]` rows boxes them. Casting those values back to `int` and `decimal` while creating the export lines unboxes them. The conversion points are marked in the source with `BOXING` and `UNBOXING` comments.

## Payroll export

Option `7` calculates payroll and calls `LegacyPayrollExporter`. The exporter writes a versioned, pipe-delimited text file with employee ID, name, role, base pay, variable pay, and total pay. Numeric formatting uses invariant culture. PAN, UAN, and GSTIN are not included.

This is a local export format defined by this application. It is not connected to an external payroll vendor; a real vendor's required schema would need to be confirmed before integrating with one.

## Current limitations

- Employee and department data are stored in memory and reset when the app restarts.
- Payroll export files are written to disk, but there is no database or roster persistence.
- There is no GUI or external payroll-vendor integration.
- Managers can hold direct reports, but the menu does not currently provide an operation to manage reports after startup.
- Reassigning an employee to a different department increments the new department's headcount without decrementing the old department's count; displayed counts can become inaccurate.
- No automated test project is configured.
