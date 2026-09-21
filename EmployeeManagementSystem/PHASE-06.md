# Phase 6 — Custom exceptions and error logging

**Status:** complete  
**How to run:** `dotnet run` from `EmployeeManagementSystem`

This phase upgrades business-rule failures from generic exceptions to named domain exceptions and records handled failures in `data/errors.log` next to the roster file.

## Exception design

`Exceptions/EmsExceptions.cs` contains a deliberately small hierarchy:

- `EmsException` is the application boundary type.
- `SalaryFloorViolationException` represents compensation below the company floor.
- `InternStipendLimitException` represents an intern-specific compensation policy breach.
- `InvalidRaiseException` represents a non-positive raise amount.
- `EmploymentEndedException` represents a salary change after a person has left.
- `AlreadyExitedException` represents a repeated resignation.
- `RosterFormatException` represents malformed persisted data and carries the source line number.

Null arguments, duplicate dictionary keys, missing departments, and invalid object usage remain built-in exceptions because they describe caller/API misuse or collection/infrastructure behavior rather than a distinct business failure.

## Logging

`Services/ErrorLog` appends a durable, human-readable record containing UTC time, operation, exception type, message, context, and stack trace. The console flow logs the validation failures it intentionally demonstrates. `RosterStore` logs file I/O failures, malformed rows, and restore failures while continuing to process valid records.

The loader catches each bad row at the file boundary, reports it, and continues. `Main` demonstrates below-floor salary, intern stipend cap, invalid raise, raise after exit, and a malformed roster row without terminating the application.

There is still no threading or menu UI; those belong to later phases.
