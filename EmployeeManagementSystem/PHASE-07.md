# Phase 7 — Threading and async

**Status:** complete  
**How to run:** `dotnet run` from `EmployeeManagementSystem`

This phase adds concurrency only where the application has a meaningful independent operation.

## Concurrency choices

- `RosterStore.SaveAsync` and `LoadAsync` use `async`/`await` with `File.WriteAllTextAsync` and `File.ReadAllLinesAsync` for roster I/O.
- `ErrorLog.RecordAsync` uses asynchronous append and a `SemaphoreSlim` so concurrent log writers produce complete entries rather than interleaved text.
- Payroll summarization runs with `Task.Run`, using the thread pool while the async roster save is in progress. Both operations are read-only over a quiescent roster after all hires, raises, and resignation have completed.
- `HeadcountMonitor` owns a dedicated background `Thread`. It samples `Employee.ActiveHeadcount`, paces itself with `Thread.Sleep`, and exposes the last sample for the main flow.
- Shutdown cancels the monitor through a `CancellationToken`, then calls `Thread.Join` in `finally` so the process never abandons the background thread.

The original synchronous `Save` and `Load` methods remain as compatibility wrappers around the async APIs. Roster mutation, hiring, assignment, raises, resignation, and reload reconstruction stay ordered and synchronous because they update shared indexes and process-wide census state; making those operations concurrent would add locking and transactional complexity without a real latency benefit in this console workflow.

The monitor reads an `int` counter only. CLR reads and writes of `int` are atomic, and the monitor never mutates the roster or counters. A reload can produce a transient diagnostic sample while the census is reconstructed, but it cannot corrupt shared state. The monitor is stopped before application shutdown.

There is still no menu UI; that belongs to Phase 8.
