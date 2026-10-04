using System.Globalization;
using System.Text.RegularExpressions;
using EmployeeManagementSystem.Enums;
using EmployeeManagementSystem.Exceptions;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Persistence;
using EmployeeManagementSystem.Policies;
using EmployeeManagementSystem.Repositories;

namespace EmployeeManagementSystem.Services
{
    internal sealed class EmployeeConsoleMenu
    {
        private readonly EmployeeRepository _employees;
        private readonly EmployeeFileStorage _fileStorage;
        private readonly ErrorLog _errorLog;
        private readonly DateTime _today;
        private readonly CancellationToken _cancellationToken;

        public EmployeeConsoleMenu(
            EmployeeRepository employees,
            EmployeeFileStorage fileStorage,
            ErrorLog errorLog,
            DateTime today,
            CancellationToken cancellationToken)
        {
            _employees = employees;
            _fileStorage = fileStorage;
            _errorLog = errorLog;
            _today = today;
            _cancellationToken = cancellationToken;
        }

        public async Task RunAsync()
        {
            bool exit = false;
            while (!exit && !_cancellationToken.IsCancellationRequested)
            {
                PrintMenu();
                int choice = ReadInt("Choose an option: ", 0, 9);
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case 1:
                            HireEmployee();
                            break;
                        case 2:
                            ListRoster();
                            break;
                        case 3:
                            AssignDepartment();
                            break;
                        case 4:
                            GiveRaise();
                            break;
                        case 5:
                            MoveAddress();
                            break;
                        case 6:
                            ResignEmployee();
                            break;
                        case 7:
                            RunPayroll();
                            break;
                        case 8:
                            await ShowRecentErrorsAsync();
                            break;
                        case 9:
                            await SaveAsync();
                            break;
                        case 0:
                            exit = true;
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    exit = true;
                }
                catch (EmsException ex)
                {
                    await ReportFailureAsync("Menu operation", ex);
                }
                catch (ArgumentException ex)
                {
                    await ReportFailureAsync("Menu input or operation", ex);
                }
                catch (InvalidOperationException ex)
                {
                    await ReportFailureAsync("Menu operation", ex);
                }
                catch (Exception ex)
                {
                    await ReportFailureAsync("Unexpected menu failure", ex);
                }

                if (!exit)
                {
                    Pause();
                }
            }
        }

        private void PrintMenu()
        {
            Console.WriteLine();
            Console.WriteLine("=== Sample Company Inc Employee Operations ===");
            Console.WriteLine("1. Hire employee");
            Console.WriteLine("2. View roster");
            Console.WriteLine("3. Assign department");
            Console.WriteLine("4. Give raise");
            Console.WriteLine("5. Preview or confirm address move");
            Console.WriteLine("6. Resign employee");
            Console.WriteLine("7. Run payroll");
            Console.WriteLine("8. View recent error log entries");
            Console.WriteLine("9. Save now");
            Console.WriteLine("0. Save and exit");
            Console.WriteLine();
        }

        private void HireEmployee()
        {
            Console.WriteLine("Hire employee");
            Console.WriteLine("1. Developer");
            Console.WriteLine("2. Manager");
            Console.WriteLine("3. Intern");
            Console.WriteLine("4. Contractor");
            Console.WriteLine("0. Cancel");
            int type = ReadInt("Employee type: ", 0, 4);
            if (type == 0)
            {
                return;
            }

            PersonName name = ReadName();
            Address address = ReadAddress();
            DateTime hireDate = ReadDate("Hire date (yyyy-MM-dd): ");
            decimal salary = ReadSalary(type);
            Employee employee;

            switch (type)
            {
                case 1:
                    employee = new Developer(
                        name,
                        address,
                        hireDate,
                        salary,
                        ReadSkill("Primary skill: "),
                        ReadFteStatutory());
                    break;
                case 2:
                    employee = new Manager(
                        name,
                        address,
                        hireDate,
                        salary,
                        ReadFteStatutory());
                    break;
                case 3:
                    employee = new Intern(
                        name,
                        address,
                        hireDate,
                        salary,
                        ReadFteStatutory());
                    break;
                default:
                    employee = new Contractor(
                        name,
                        ReadContractorRole(),
                        address,
                        hireDate,
                        salary,
                        new { Gstin = ReadGstin("GSTIN: ") });
                    break;
            }

            _employees.Hire(employee);
            Console.WriteLine($"Hired {employee.FormatNotificationLabel()}.");
            if (ReadYesNo("Assign a department now? (y/n): "))
            {
                Department department = ReadDepartment();
                _employees.AssignToDepartment(employee, department);
            }
        }

        private void ListRoster()
        {
            Console.WriteLine("1. Everyone");
            Console.WriteLine("2. Active staff");
            Console.WriteLine("3. Leavers");
            Console.WriteLine("4. By department");
            int choice = ReadInt("View: ", 1, 4);
            List<Employee> people;

            switch (choice)
            {
                case 2:
                    people = _employees.ListActive();
                    break;
                case 3:
                    people = _employees.ListLeavers();
                    break;
                case 4:
                    people = _employees.ListByDepartment(ReadDepartment().Id);
                    break;
                default:
                    people = _employees.ListInHireOrder();
                    break;
            }

            if (people.Count == 0)
            {
                Console.WriteLine("No matching employees.");
                return;
            }

            for (int i = 0; i < people.Count; i++)
            {
                Employee employee = people[i];
                Console.WriteLine($"  {employee}");
                Console.WriteLine($"    {employee.DescribeStatutory()}");
                Console.WriteLine($"    {employee.GetEmploymentSummary(_today)}");
            }
        }

        private void AssignDepartment()
        {
            Employee employee = ReadEmployee();
            Department department = ReadDepartment();
            _employees.AssignToDepartment(employee, department);
            Console.WriteLine($"{employee.Name} assigned to {department.Name}.");
        }

        private void GiveRaise()
        {
            Employee employee = ReadEmployee();
            decimal amount;
            while (true)
            {
                amount = ReadDecimal("Raise amount in INR: ");
                if (amount <= 0)
                {
                    Console.WriteLine("Raise amount must be greater than zero.");
                    continue;
                }

                break;
            }

            string reason = ReadRequired("Reason: ");
            _employees.RecordRaise(employee, amount, reason);
            Console.WriteLine($"New salary: {employee.MonthlySalaryInr:C}.");
        }

        private void MoveAddress()
        {
            Employee employee = ReadEmployee();
            Console.WriteLine($"Current address: {employee.HomeAddress}");
            Address proposed = employee.ProposeRelocation(
                ReadRequired("New street: "),
                ReadCity("New city: "),
                ReadState("New state: "),
                ReadPostalCode("New postal code: "));
            Console.WriteLine($"Proposed address: {proposed}");
            if (ReadYesNo("Confirm this move? (y/n): "))
            {
                employee.ConfirmRelocation(proposed);
                Console.WriteLine("Address updated.");
            }
            else
            {
                Console.WriteLine("Address change discarded.");
            }
        }

        private void ResignEmployee()
        {
            Employee employee = ReadEmployee();
            DateTime lastDay = ReadDate("Last day (yyyy-MM-dd): ");
            _employees.RecordResignation(employee, lastDay);
            Console.WriteLine($"{employee.Name} recorded as resigned.");
        }

        private void RunPayroll()
        {
            List<Employee> active = _employees.ListActive();
            if (active.Count == 0)
            {
                Console.WriteLine("No active staff to pay.");
                return;
            }

            for (int i = 0; i < active.Count; i++)
            {
                Console.WriteLine($"  {active[i].DescribeCompensation()}");
            }

            (int people, decimal basePay, decimal variablePay) = _employees.SummarizeActivePay();
            Console.WriteLine($"Totals: {people} people, {basePay:C} base + {variablePay:C} variable.");
        }

        private async Task ShowRecentErrorsAsync()
        {
            List<string> entries = await _errorLog.ReadRecentAsync(5, _cancellationToken);
            if (entries.Count == 0)
            {
                Console.WriteLine("No error log entries.");
                return;
            }

            Console.WriteLine("Recent errors");
            for (int i = 0; i < entries.Count; i++)
            {
                Console.WriteLine(entries[i]);
            }
        }

        private async Task SaveAsync()
        {
            (int departments, int people, int badges) = await _fileStorage.SaveAsync(
                _employees,
                _cancellationToken);
            Console.WriteLine($"Saved {departments} departments, {people} people, {badges} badges.");
        }

        private async Task ReportFailureAsync(string operation, Exception exception)
        {
            Console.WriteLine($"Could not complete that operation: {exception.Message}");
            await _errorLog.RecordAsync(
                operation,
                exception,
                "Interactive console request",
                _cancellationToken);
        }

        private Employee ReadEmployee()
        {
            while (true)
            {
                int staffId = ReadInt("Staff id: ", 1, int.MaxValue);
                Employee? employee = _employees.FindByStaffId(staffId);
                if (employee != null)
                {
                    return employee;
                }

                Console.WriteLine("No employee has that staff id. Try again.");
            }
        }

        private Department ReadDepartment()
        {
            List<Department> departments = _employees.ListDepartments();
            if (departments.Count == 0)
            {
                throw new InvalidOperationException("No departments are registered.");
            }

            Console.WriteLine("Departments");
            for (int i = 0; i < departments.Count; i++)
            {
                Console.WriteLine($"  {departments[i].Id}. {departments[i].Name} ({departments[i].CurrentHeadcount} people)");
            }

            while (true)
            {
                int departmentId = ReadInt("Department id: ", 1, int.MaxValue);
                Department? department = _employees.FindDepartment(departmentId);
                if (department != null)
                {
                    return department;
                }

                Console.WriteLine("No department has that id. Try again.");
            }
        }

        private static PersonName ReadName()
        {
            string first = ReadNamePart("First name: ", isRequired: true)!;
            string last = ReadNamePart("Last name: ", isRequired: true)!;
            string? middle = ReadNamePart("Middle name (optional): ", isRequired: false);
            return new PersonName(first, last, middle);
        }

        private static string? ReadNamePart(string prompt, bool isRequired)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = (Console.ReadLine() ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (!isRequired)
                    {
                        return null;
                    }

                    Console.WriteLine("A value is required.");
                    continue;
                }

                if (ContainsDigits(value))
                {
                    Console.WriteLine("Name cannot contain numbers. Please enter letters only.");
                    continue;
                }

                if (!PersonName.IsValidName(value))
                {
                    Console.WriteLine("Name can only contain letters, spaces, hyphens, and apostrophes.");
                    continue;
                }

                return value;
            }
        }

        private static Address ReadAddress()
        {
            return new Address(
                ReadRequired("Street: "),
                ReadCity("City: "),
                ReadState("State: "),
                ReadPostalCode("Postal code: "));
        }

        private static string ReadCity(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = (Console.ReadLine() ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("City is required.");
                    continue;
                }

                if (ContainsDigits(value))
                {
                    Console.WriteLine("City cannot contain numbers. Please enter a valid city name.");
                    continue;
                }

                if (!IsValidLocationName(value))
                {
                    Console.WriteLine("City can only contain letters, spaces, and hyphens.");
                    continue;
                }

                return value;
            }
        }

        private static string ReadState(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = (Console.ReadLine() ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("State is required.");
                    continue;
                }

                if (ContainsDigits(value))
                {
                    Console.WriteLine("State cannot contain numbers. Please enter a valid state name.");
                    continue;
                }

                if (!IsValidLocationName(value))
                {
                    Console.WriteLine("State can only contain letters, spaces, and hyphens.");
                    continue;
                }

                return value;
            }
        }

        private static string ReadPostalCode(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = (Console.ReadLine() ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("Postal code is required.");
                    continue;
                }

                if (Regex.IsMatch(value, @"^\d{6}$"))
                {
                    return value;
                }

                Console.WriteLine("Enter a valid 6-digit postal PIN code (e.g., 560103).");
            }
        }

        private static object ReadFteStatutory()
        {
            return new
            {
                Pan = ReadPan("PAN: "),
                Uan = ReadUan("UAN: ")
            };
        }

        private static string ReadPan(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = (Console.ReadLine() ?? string.Empty).Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("PAN is required.");
                    continue;
                }

                if (Regex.IsMatch(value, @"^[A-Z]{5}[0-9]{4}[A-Z]$"))
                {
                    return value;
                }

                Console.WriteLine("Enter a valid 10-character PAN (5 uppercase letters, 4 digits, 1 letter, e.g., ABCDE1234F).");
            }
        }

        private static string ReadUan(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = (Console.ReadLine() ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("UAN is required.");
                    continue;
                }

                if (Regex.IsMatch(value, @"^\d{12}$"))
                {
                    return value;
                }

                Console.WriteLine("Enter a valid 12-digit UAN (e.g., 100123456789).");
            }
        }

        private static string ReadGstin(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = (Console.ReadLine() ?? string.Empty).Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("GSTIN is required.");
                    continue;
                }

                if (Regex.IsMatch(value, @"^\d{2}[A-Z]{5}\d{4}[A-Z]{1}[1-9A-Z]{1}[A-Z0-9]{1}[A-Z0-9]{1}$"))
                {
                    return value;
                }

                Console.WriteLine("Enter a valid 15-character GSTIN (e.g., 29ABCDE1234F1Z5).");
            }
        }

        private static decimal ReadSalary(int employeeType)
        {
            if (employeeType == 3) // Intern
            {
                while (true)
                {
                    decimal amount = ReadDecimal($"Monthly stipend in INR ({CompanyRules.MinimumMonthlySalaryInr:N0} - {CompanyRules.MaximumInternStipendInr:N0}): ");
                    if (amount < CompanyRules.MinimumMonthlySalaryInr)
                    {
                        Console.WriteLine($"Stipend cannot be below company floor of {CompanyRules.MinimumMonthlySalaryInr:C}.");
                        continue;
                    }

                    if (amount > CompanyRules.MaximumInternStipendInr)
                    {
                        Console.WriteLine($"Intern stipend cannot exceed maximum limit of {CompanyRules.MaximumInternStipendInr:C}.");
                        continue;
                    }

                    return amount;
                }
            }
            else
            {
                while (true)
                {
                    decimal amount = ReadDecimal($"Monthly salary/rate in INR (minimum {CompanyRules.MinimumMonthlySalaryInr:N0}): ");
                    if (amount < CompanyRules.MinimumMonthlySalaryInr)
                    {
                        Console.WriteLine($"Salary cannot be below company floor of {CompanyRules.MinimumMonthlySalaryInr:C}.");
                        continue;
                    }

                    return amount;
                }
            }
        }

        private static string ReadSkill(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = (Console.ReadLine() ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("Skill is required.");
                    continue;
                }

                bool allDigits = true;
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsDigit(value[i]))
                    {
                        allDigits = false;
                        break;
                    }
                }

                if (allDigits)
                {
                    Console.WriteLine("Skill cannot be just numbers. Please enter a valid skill (e.g., C#, React, Azure).");
                    continue;
                }

                return value;
            }
        }

        private static bool ContainsDigits(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsDigit(value[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsValidLocationName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            bool hasLetter = false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsLetter(c))
                {
                    hasLetter = true;
                }
                else if (c != ' ' && c != '-' && c != '.' && c != '\'')
                {
                    return false;
                }
            }

            return hasLetter;
        }

        private static EmployeeRole ReadContractorRole()
        {
            Console.WriteLine("Contractor role: 1 Developer, 2 TeamLead, 3 Manager, 4 HR");
            int choice = ReadInt("Role: ", 1, 4);
            switch (choice)
            {
                case 2:
                    return EmployeeRole.TeamLead;
                case 3:
                    return EmployeeRole.Manager;
                case 4:
                    return EmployeeRole.HR;
                default:
                    return EmployeeRole.Developer;
            }
        }

        private static string ReadRequired(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = Console.ReadLine() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }

                Console.WriteLine("A value is required.");
            }
        }

        private static string? ReadOptional(string prompt)
        {
            Console.Write(prompt);
            string value = Console.ReadLine() ?? string.Empty;
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static int ReadInt(string prompt, int minimum, int maximum)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = Console.ReadLine() ?? string.Empty;
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
                    && result >= minimum
                    && result <= maximum)
                {
                    return result;
                }

                Console.WriteLine($"Enter a whole number from {minimum} to {maximum}.");
            }
        }

        private static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = Console.ReadLine() ?? string.Empty;
                if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result;
                }

                Console.WriteLine("Enter a valid decimal amount.");
            }
        }

        private static DateTime ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = Console.ReadLine() ?? string.Empty;
                if (DateTime.TryParseExact(
                    value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime result))
                {
                    return result;
                }

                Console.WriteLine("Enter a date in yyyy-MM-dd format.");
            }
        }

        private static bool ReadYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
                if (value == "y" || value == "yes")
                {
                    return true;
                }

                if (value == "n" || value == "no")
                {
                    return false;
                }

                Console.WriteLine("Enter y or n.");
            }
        }

        private static void Pause()
        {
            Console.WriteLine();
            Console.Write("Press Enter to return to the menu...");
            Console.ReadLine();
        }
    }
}
