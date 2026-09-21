using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Notifications;
using EmployeeManagementSystem.Persistence;
using EmployeeManagementSystem.Repositories;
using EmployeeManagementSystem.Services;

namespace EmployeeManagementSystem
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            DateTime today = DateTime.Today;
            string dataPath = Path.Combine(AppContext.BaseDirectory, "data");
            string employeeFilePath = Path.Combine(dataPath, "workforce.txt");
            var errorLog = new ErrorLog(Path.Combine(dataPath, "errors.log"));
            using var fileStorage = new EmployeeFileStorage(employeeFilePath, errorLog);
            using var cancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellation.Token;

            EmployeeRepository employees;
            try
            {
                employees = await fileStorage.LoadAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                await errorLog.RecordAsync(
                    "Application startup",
                    ex,
                    $"Could not load employee file at {employeeFilePath}",
                    cancellationToken);
                Console.WriteLine($"Could not load employee data: {ex.Message}");
                employees = new EmployeeRepository();
            }

            EnsureDefaultDepartments(employees);
            var employeeNotificationService = new EmployeeNotificationService(today);
            employeeNotificationService.Attach(employees);
            var employeeHeadcountMonitor = new EmployeeHeadcountMonitor(
                cancellationToken,
                TimeSpan.FromSeconds(1));
            employeeHeadcountMonitor.Start();

            Console.WriteLine("Sample Company Inc — Employee Operations");
            Console.WriteLine($"HQ: {Policies.CompanyRules.Headquarters}");
            Console.WriteLine($"Loaded {employees.ListInHireOrder().Count} people and {employees.ListDepartments().Count} departments.");
            Console.WriteLine($"Background headcount monitor started. Error log: {errorLog.LogPath}");

            try
            {
                var menu = new EmployeeConsoleMenu(
                    employees,
                    fileStorage,
                    errorLog,
                    today,
                    cancellationToken);
                await menu.RunAsync();
            }
            finally
            {
                cancellation.Cancel();
                employeeHeadcountMonitor.Join();
                Console.WriteLine(
                    $"Background monitor stopped after {employeeHeadcountMonitor.SampleCount} samples.");

                try
                {
                    (int departments, int people, int badges) = await fileStorage.SaveAsync(employees);
                    Console.WriteLine($"Saved {departments} departments, {people} people, {badges} badges.");
                }
                catch (Exception ex)
                {
                    await errorLog.RecordAsync(
                        "Application shutdown save",
                        ex,
                        $"Could not save employee file at {employeeFilePath}");
                    Console.WriteLine($"Could not save employee data on exit: {ex.Message}");
                }
            }
        }

        private static void EnsureDefaultDepartments(EmployeeRepository employees)
        {
            if (employees.FindDepartment(10) == null)
            {
                employees.RegisterDepartment(new Department(10, "Engineering"));
            }

            if (employees.FindDepartment(20) == null)
            {
                employees.RegisterDepartment(new Department(20, "People Ops"));
            }
        }
    }
}
