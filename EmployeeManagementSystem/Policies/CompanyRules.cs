using EmployeeManagementSystem.Enums;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Policies
{
    // Company-wide rules live on the type, not on any one employee.
    // Instance data (a person's salary, address) cannot answer "what is our
    // probation policy?" — that is an organisation fact.
    public static class CompanyRules
    {
        public const decimal MinimumMonthlySalaryInr = 15000m;
        public const decimal MaximumInternStipendInr = 25000m;
        public const int StandardProbationDays = 90;

        public static Address Headquarters { get; } = new Address(
            "14 Embassy Tech Village",
            "Bengaluru",
            "KA",
            "560103");

        public static DateTime? ComputeProbationEnd(ContractType contract, DateTime hireDate)
        {
            // Contractors are engaged for a term, not a probation cycle,
            // so "probation end" is genuinely absent — not a dummy date.
            if (contract == ContractType.Contractor)
            {
                return null;
            }

            return hireDate.AddDays(StandardProbationDays);
        }

        public static bool MeetsSalaryFloor(decimal monthlySalaryInr)
        {
            return monthlySalaryInr >= MinimumMonthlySalaryInr;
        }
    }
}
