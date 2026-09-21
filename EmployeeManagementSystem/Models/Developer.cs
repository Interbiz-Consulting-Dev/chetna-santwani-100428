using EmployeeManagementSystem.Enums;

namespace EmployeeManagementSystem.Models
{
    public class Developer : Employee
    {
        public string PrimarySkill { get; }

        public Developer(
            PersonName name,
            Address homeAddress,
            DateTime hireDate,
            decimal monthlySalaryInr,
            string primarySkill,
            object statutory)
            : base(name, EmployeeRole.Developer, ContractType.FullTime, homeAddress, hireDate, monthlySalaryInr, statutory)
        {
            PrimarySkill = primarySkill;
        }

        internal Developer(EmployeeState state, object statutory)
            : base(state, statutory)
        {
            PrimarySkill = state.Extra;
        }

        public override int NoticePeriodDays => 30;

        public override decimal ComputeVariablePay()
        {
            decimal rate = 0.08m;
            if (!string.IsNullOrEmpty(PrimarySkill)
                && PrimarySkill.IndexOf("C#", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                rate = 0.10m;
            }

            return MonthlySalaryInr * rate;
        }
    }
}
