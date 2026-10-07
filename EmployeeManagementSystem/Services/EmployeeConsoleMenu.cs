using System.Globalization;
using System.Text.RegularExpressions;
using EmployeeManagementSystem.Enums;
using EmployeeManagementSystem.Exceptions;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Policies;

namespace EmployeeManagementSystem.Services
{
    public class EmployeeConsoleMenu
    {
        private readonly List<Employee> _employees = new List<Employee>();
        private readonly List<Department> _departments = new List<Department>();
        private readonly DateTime _today = DateTime.Today;

        public EmployeeConsoleMenu()
        {
            SeedInitialData();
        }

        private void SeedInitialData()
        {
            // Seed initial departments
            var engineering = new Department(10, "Engineering");
            var peopleOps = new Department(20, "People Ops");
            var sales = new Department(30, "Sales & Marketing");

            _departments.Add(engineering);
            _departments.Add(peopleOps);
            _departments.Add(sales);

            // Seed sample employees
            var dev = new Developer(
                new PersonName("Rahul", "Sharma"),
                new Address("12 MG Road", "Bengaluru", "Karnataka", "560001"),
                DateTime.Today.AddMonths(-10),
                65000m,
                "C# / .NET",
                new { Pan = "ABCDE1234F", Uan = "100123456789" });
            dev.AssignTo(engineering);
            _employees.Add(dev);

            var mgr = new Manager(
                new PersonName("Priya", "Patel"),
                new Address("45 Indiranagar", "Bengaluru", "Karnataka", "560038"),
                DateTime.Today.AddYears(-2),
                120000m,
                new { Pan = "XYZAB5678C", Uan = "100987654321" });
            mgr.AssignTo(engineering);
            mgr.AddDirectReport(dev);
            _employees.Add(mgr);

            var intern = new Intern(
                new PersonName("Aarav", "Mehta"),
                new Address("88 Koramangala", "Bengaluru", "Karnataka", "560034"),
                DateTime.Today.AddMonths(-1),
                22000m,
                new { Pan = "PQRST9012D", Uan = "100555666777" });
            intern.AssignTo(peopleOps);
            _employees.Add(intern);
        }

        public void Run()
        {
            bool exit = false;
            while (!exit)
            {
                PrintMenu();
                int choice = ReadInt("Choose an option: ", 0, 7);
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case 1:
                            HireEmployee();
                            break;
                        case 2:
                            ViewEmployeeList();
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
                        case 0:
                            exit = true;
                            break;
                    }
                }
                catch (EmsException ex)
                {
                    Console.WriteLine($"\n[Error]: {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"\n[Input Error]: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[Unexpected Error]: {ex.Message}");
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
            Console.WriteLine("==================================================");
            Console.WriteLine("       Sample Company Inc - Employee System       ");
            Console.WriteLine("==================================================");
            Console.WriteLine("1. Hire employee");
            Console.WriteLine("2. View employee list");
            Console.WriteLine("3. Assign department");
            Console.WriteLine("4. Give raise");
            Console.WriteLine("5. Update employee address");
            Console.WriteLine("6. Resign employee");
            Console.WriteLine("7. Run payroll");
            Console.WriteLine("0. Exit");
            Console.WriteLine("==================================================");
        }

        private void HireEmployee()
        {
            Console.WriteLine("=== Hire New Employee ===");
            Console.WriteLine("1. Developer");
            Console.WriteLine("2. Manager");
            Console.WriteLine("3. Intern");
            Console.WriteLine("4. Contractor");
            Console.WriteLine("0. Cancel");
            int type = ReadInt("Select employee type: ", 0, 4);
            if (type == 0)
            {
                Console.WriteLine("Hiring cancelled.");
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

            _employees.Add(employee);
            Console.WriteLine($"\nSuccessfully hired {employee.Name} as {employee.Role} (Staff ID: {employee.Id})!");

            if (ReadYesNo("Assign a department now? (y/n): "))
            {
                Department department = ReadDepartment();
                employee.AssignTo(department);
                Console.WriteLine($"{employee.Name} assigned to {department.Name}.");
            }
        }

        private void ViewEmployeeList()
        {
            Console.WriteLine("=== View Employee List ===");
            Console.WriteLine("1. All employees");
            Console.WriteLine("2. Active employees only");
            Console.WriteLine("3. Resigned employees only");
            Console.WriteLine("4. Filter by department");
            int choice = ReadInt("Choose view: ", 1, 4);
            Console.WriteLine();

            List<Employee> list;
            switch (choice)
            {
                case 2:
                    list = _employees.FindAll(e => e.Status == EmploymentStatus.Active);
                    break;
                case 3:
                    list = _employees.FindAll(e => e.Status == EmploymentStatus.Resigned);
                    break;
                case 4:
                    Department dept = ReadDepartment();
                    list = _employees.FindAll(e => e.DepartmentId == dept.Id);
                    break;
                default:
                    list = new List<Employee>(_employees);
                    break;
            }

            if (list.Count == 0)
            {
                Console.WriteLine("No matching employees found.");
                return;
            }

            Console.WriteLine($"Total employees: {list.Count}");
            Console.WriteLine("--------------------------------------------------------------------------------");
            for (int i = 0; i < list.Count; i++)
            {
                Employee emp = list[i];
                string deptName = "Unassigned";
                if (emp.DepartmentId.HasValue)
                {
                    Department? d = _departments.Find(dep => dep.Id == emp.DepartmentId.Value);
                    if (d != null)
                    {
                        deptName = d.Name;
                    }
                }

                Console.WriteLine($"[{emp.Id}] {emp.Name} | {emp.Role} | {emp.Contract} | {emp.Status} | Dept: {deptName}");
                Console.WriteLine($"    Address: {emp.HomeAddress}");
                Console.WriteLine($"    Statutory: {emp.DescribeStatutory()}");
                Console.WriteLine($"    Base Salary: {emp.MonthlySalaryInr:C} | Notice: {emp.NoticePeriodDays} days");
                Console.WriteLine($"    Status: {emp.GetEmploymentSummary(_today)}");
                Console.WriteLine("--------------------------------------------------------------------------------");
            }
        }

        private void AssignDepartment()
        {
            Console.WriteLine("=== Assign Department ===");
            Employee employee = ReadEmployee();
            Department department = ReadDepartment();
            employee.AssignTo(department);
            Console.WriteLine($"\n{employee.Name} (ID: {employee.Id}) assigned to {department.Name}.");
        }

        private void GiveRaise()
        {
            Console.WriteLine("=== Give Salary Raise ===");
            Employee employee = ReadEmployee();
            if (employee.Status != EmploymentStatus.Active)
            {
                Console.WriteLine("Cannot give raise to an inactive/resigned employee.");
                return;
            }

            Console.WriteLine($"Current Salary: {employee.MonthlySalaryInr:C}");
            decimal amount;
            while (true)
            {
                amount = ReadDecimal("Enter raise amount in INR: ");
                if (amount <= 0)
                {
                    Console.WriteLine("Raise amount must be greater than zero.");
                    continue;
                }

                break;
            }

            string reason = ReadRequired("Reason for raise: ");
            employee.GiveRaise(amount, reason);
            Console.WriteLine($"\nRaise applied! New salary for {employee.Name}: {employee.MonthlySalaryInr:C}");
        }

        private void MoveAddress()
        {
            Console.WriteLine("=== Update Address ===");
            Employee employee = ReadEmployee();
            Console.WriteLine($"Current Address: {employee.HomeAddress}\n");

            Address proposed = employee.ProposeRelocation(
                ReadRequired("New street: "),
                ReadCity("New city: "),
                ReadState("New state: "),
                ReadPostalCode("New postal code: "));

            Console.WriteLine($"\nProposed Address: {proposed}");
            if (ReadYesNo("Confirm this address change? (y/n): "))
            {
                employee.ConfirmRelocation(proposed);
                Console.WriteLine("Address updated successfully.");
            }
            else
            {
                Console.WriteLine("Address update cancelled.");
            }
        }

        private void ResignEmployee()
        {
            Console.WriteLine("=== Resign Employee ===");
            Employee employee = ReadEmployee();
            if (employee.Status == EmploymentStatus.Resigned)
            {
                Console.WriteLine("This employee has already resigned.");
                return;
            }

            DateTime lastDay = ReadDate("Last working day (yyyy-MM-dd): ");
            employee.Resign(lastDay);
            Console.WriteLine($"\n{employee.Name} (ID: {employee.Id}) has been marked as resigned on {lastDay:yyyy-MM-dd}.");
        }

        private void RunPayroll()
        {
            Console.WriteLine("=== Run Monthly Payroll ===");
            List<Employee> active = _employees.FindAll(e => e.Status == EmploymentStatus.Active);
            if (active.Count == 0)
            {
                Console.WriteLine("No active employees on the books.");
                return;
            }

            decimal totalBase = 0m;
            decimal totalVariable = 0m;

            Console.WriteLine("{0,-6} {1,-20} {2,-12} {3,15} {4,15} {5,15}", "ID", "Name", "Role", "Base Pay", "Variable Pay", "Total Payout");
            Console.WriteLine(new string('-', 90));

            for (int i = 0; i < active.Count; i++)
            {
                Employee emp = active[i];
                decimal variable = emp.ComputeVariablePay();
                decimal total = emp.MonthlySalaryInr + variable;

                totalBase += emp.MonthlySalaryInr;
                totalVariable += variable;

                Console.WriteLine("{0,-6} {1,-20} {2,-12} {3,15:C} {4,15:C} {5,15:C}",
                    emp.Id,
                    emp.Name.Full.Length > 20 ? emp.Name.Full.Substring(0, 17) + "..." : emp.Name.Full,
                    emp.Role,
                    emp.MonthlySalaryInr,
                    variable,
                    total);
            }

            Console.WriteLine(new string('-', 90));
            Console.WriteLine($"Active Staff: {active.Count}");
            Console.WriteLine($"Total Base Pay:      {totalBase:C}");
            Console.WriteLine($"Total Variable Pay:  {totalVariable:C}");
            Console.WriteLine($"Grand Total Payout:  {(totalBase + totalVariable):C}");

            string exportPath = LegacyPayrollExporter.Export(active, _today);
            Console.WriteLine($"Legacy payroll export saved to: {exportPath}");
        }

        private Employee ReadEmployee()
        {
            while (true)
            {
                int staffId = ReadInt("Enter Staff ID: ", 1, int.MaxValue);
                Employee? emp = _employees.Find(e => e.Id == staffId);
                if (emp != null)
                {
                    return emp;
                }

                Console.WriteLine($"No employee found with Staff ID {staffId}. Please try again.");
            }
        }

        private Department ReadDepartment()
        {
            if (_departments.Count == 0)
            {
                throw new InvalidOperationException("No departments exist.");
            }

            Console.WriteLine("\nAvailable Departments:");
            for (int i = 0; i < _departments.Count; i++)
            {
                Console.WriteLine($"  ID { _departments[i].Id}: {_departments[i].Name} ({_departments[i].CurrentHeadcount} people)");
            }

            while (true)
            {
                int id = ReadInt("Enter Department ID: ", 1, int.MaxValue);
                Department? dept = _departments.Find(d => d.Id == id);
                if (dept != null)
                {
                    return dept;
                }

                Console.WriteLine("Invalid Department ID. Please try again.");
            }
        }

        // ==========================================
        // Input Validation Helpers
        // ==========================================

        private static PersonName ReadName()
        {
            string first = ReadNamePart("First name: ", isRequired: true)!;
            string last = ReadNamePart("Last name: ", isRequired: true)!;
            string? middle = ReadNamePart("Middle name (optional, press Enter to skip): ", isRequired: false);
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

        private static EmployeeRole ReadContractorRole()
        {
            Console.WriteLine("Contractor role: 1. Developer, 2. TeamLead, 3. Manager, 4. HR");
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

        private static void Pause()
        {
            Console.WriteLine();
            Console.Write("Press Enter to return to the menu...");
            Console.ReadLine();
        }
    }
}
