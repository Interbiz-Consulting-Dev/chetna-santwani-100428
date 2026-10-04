# Employee Management System — Complete Guide

## What Is This App?

A clean, beginner-friendly **C# console application** that demonstrates core **Object-Oriented Programming (OOP)** concepts. It manages an in-memory company workforce: hiring employees, viewing rosters, assigning departments, giving salary raises, updating addresses, offboarding/resigning staff, and calculating payroll.

---

## Architecture at a Glance

```
┌──────────────────────────────────────────────────────────────┐
│                        Program.cs                            │
│           (Simple Entry Point: launches Console Menu)        │
└──────────────────────────────┬───────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                    EmployeeConsoleMenu                       │
│    (In-memory List<Employee>, List<Department>, UI & input)  │
└──────────────────────────────┬───────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                          Models                              │
│  Employee (abstract base class)                              │
│  ├── Developer                                               │
│  ├── Manager (+ Manager.Team)                                │
│  ├── Intern                                                  │
│  └── Contractor                                              │
│                                                              │
│  Value Objects: PersonName, Address, Department              │
│  Enums: EmployeeRole, ContractType, EmploymentStatus         │
│  Policies: CompanyRules                                      │
└──────────────────────────────────────────────────────────────┘
```

---

## Core OOP Concepts Demonstrated

### 1. Inheritance
- `Employee` is an **abstract base class** containing shared properties and methods (`Id`, `Name`, `Role`, `Contract`, `Status`, `HomeAddress`, `HireDate`, `MonthlySalaryInr`, `AssignTo()`, `Resign()`, `GiveRaise()`).
- `Developer`, `Manager`, `Intern`, and `Contractor` derive from `Employee`, inheriting its members while adding role-specific behavior.

### 2. Polymorphism
- The abstract method `ComputeVariablePay()` is overridden by each employee subtype:
  - **Developer:** 8% bonus (10% if primary skill includes "C#").
  - **Manager:** 3% base salary per direct report.
  - **Intern:** ₹0 bonus.
  - **Contractor:** 5% monthly retainer.
- The payroll engine iterates through a simple `List<Employee>` and calls `employee.ComputeVariablePay()` polymorphically without needing `if/else` type checking.

### 3. Encapsulation & Validation
- Private fields and property accessors protect sensitive state.
- **Strict Input Validation:**
  - Names cannot contain numbers/digits (`ContainsDigits()`, `IsValidName()`).
  - Addresses validate that City and State do not contain numbers, and Postal Code is 6 digits.
  - Indian statutory identifiers validate PAN format (`ABCDE1234F`), UAN format (12 digits), and GSTIN format (15 characters).
  - Salaries cannot drop below the company floor (`CompanyRules.MinimumMonthlySalaryInr`), and intern stipends cannot exceed the maximum cap.

### 4. Value Types vs Reference Types
- `PersonName` and `Address` are immutable `readonly struct`s (copy-by-value semantics).
- `Employee` and `Department` are `class`es (reference types with distinct identities).

### 5. Custom Exceptions
- Clean, domain-specific exceptions deriving from `EmsException`:
  - `SalaryFloorViolationException`
  - `InternStipendLimitException`
  - `InvalidRaiseException`
  - `EmploymentEndedException`
  - `AlreadyExitedException`
  - `FieldValidationException`

---

## Console Menu Operations

| Option | Action | Details |
|---|---|---|
| **1** | **Hire Employee** | Creates a Developer, Manager, Intern, or Contractor with validated input. |
| **2** | **View Employee List** | Displays all staff, active only, leavers only, or filtered by department. |
| **3** | **Assign Department** | Links an employee to a department and updates department headcount. |
| **4** | **Give Salary Raise** | Validates positive amount and reason, then updates base salary. |
| **5** | **Update Address** | Two-step address update: preview proposed relocation then confirm. |
| **6** | **Resign Employee** | Sets last working day and transitions status from Active to Resigned. |
| **7** | **Run Payroll** | Computes monthly base pay + polymorphic variable bonus for all active staff. |
| **0** | **Exit** | Terminates the application. |
