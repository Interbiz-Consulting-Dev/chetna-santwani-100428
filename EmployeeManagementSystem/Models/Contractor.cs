using EmployeeManagementSystem.Enums;

namespace EmployeeManagementSystem.Models
{
    public class Contractor : Employee
    {
        public Contractor(
            PersonName name,
            EmployeeRole role,
            Address homeAddress,
            DateTime hireDate,
            decimal monthlyRateInr,
            object statutory)
            : base(name, role, ContractType.Contractor, homeAddress, hireDate, monthlyRateInr, statutory)
        {
        }

        internal Contractor(EmployeeState state, object statutory)
            : base(state, statutory)
        {
        }

        public override int NoticePeriodDays => 15;

        // Contractors are not on the FTE bonus plan; they get a small monthly
        // retainer instead of performance variable pay.
        public override decimal ComputeVariablePay()
        {
            return MonthlySalaryInr * 0.05m;
        }

        public override string DescribeStatutory()
        {
            return $"GSTIN {Statutory.Gstin}";
        }
    }
}
