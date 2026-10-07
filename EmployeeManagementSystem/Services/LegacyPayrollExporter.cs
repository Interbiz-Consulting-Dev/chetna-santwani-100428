using System.Collections;
using System.Globalization;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Services
{
    internal static class LegacyPayrollExporter
    {
        public static string Export(IReadOnlyList<Employee> employees, DateTime payrollDate)
        {
            ArrayList payrollRows = new ArrayList(); // Legacy payroll format uses untyped positional records.
            for (int i = 0; i < employees.Count; i++)
            {
                Employee employee = employees[i];
                decimal variablePay = employee.ComputeVariablePay();
                decimal totalPay = employee.MonthlySalaryInr + variablePay;

                payrollRows.Add(new object[]
                {
                    employee.Id,
                    employee.Name.Full,
                    employee.Role.ToString(),
                    employee.MonthlySalaryInr,
                    variablePay,
                    totalPay
                }); // BOXING: int and decimal values become object elements.
            }

            string[] lines = new string[payrollRows.Count + 2];
            lines[0] = $"EMS-LEGACY-PAYROLL|1|{payrollDate.ToString("yyyy-MM", CultureInfo.InvariantCulture)}";
            lines[1] = "EmployeeId|EmployeeName|Role|BasePay|VariablePay|TotalPay";

            for (int i = 0; i < payrollRows.Count; i++)
            {
                object[] row = (object[])payrollRows[i]!;
                int staffId = (int)row[0]!; // UNBOXING: boxed int -> int.
                string employeeName = (string)row[1]!;
                string role = (string)row[2]!;
                decimal basePay = (decimal)row[3]!; // UNBOXING: boxed decimal -> decimal.
                decimal variablePay = (decimal)row[4]!;
                decimal totalPay = (decimal)row[5]!;

                lines[i + 2] = string.Join('|',
                    staffId.ToString(CultureInfo.InvariantCulture),
                    EscapeField(employeeName),
                    role,
                    basePay.ToString("0.00", CultureInfo.InvariantCulture),
                    variablePay.ToString("0.00", CultureInfo.InvariantCulture),
                    totalPay.ToString("0.00", CultureInfo.InvariantCulture));
            }

            string exportDirectory = Path.Combine(Environment.CurrentDirectory, "data");
            Directory.CreateDirectory(exportDirectory);

            string runStamp = DateTime.Now.ToString("yyyyMMdd-HHmmssfff", CultureInfo.InvariantCulture);
            string payrollMonth = payrollDate.ToString("yyyyMM", CultureInfo.InvariantCulture);
            string exportPath = Path.Combine(exportDirectory, $"legacy-payroll-{payrollMonth}-{runStamp}.txt");
            File.WriteAllLines(exportPath, lines);
            return exportPath;
        }

        private static string EscapeField(string value)
        {
            return value.Replace('|', ' ').Replace('\r', ' ').Replace('\n', ' ');
        }
    }
}