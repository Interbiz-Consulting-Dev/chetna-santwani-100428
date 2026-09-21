using EmployeeManagementSystem.Enums;
using EmployeeManagementSystem.Exceptions;
using EmployeeManagementSystem.Policies;

namespace EmployeeManagementSystem.Models
{
    public class Intern : Employee
    {
        public Intern(
            PersonName name,
            Address homeAddress,
            DateTime hireDate,
            decimal monthlyStipendInr,
            object statutory)
            : base(name, EmployeeRole.Intern, ContractType.Intern, homeAddress, hireDate, monthlyStipendInr, statutory)
        {
        }

        internal Intern(EmployeeState state, object statutory)
            : base(state, statutory)
        {
        }

        public override int NoticePeriodDays => 7;

        public override decimal ComputeVariablePay()
        {
            return 0m;
        }

        public override string FormatNotificationLabel()
        {
            return $"{base.FormatNotificationLabel()} (intern track)";
        }

        protected override void SetMonthlySalary(decimal monthlySalaryInr)
        {
            if (monthlySalaryInr > CompanyRules.MaximumInternStipendInr)
            {
                throw new InternStipendLimitException(
                    monthlySalaryInr,
                    CompanyRules.MaximumInternStipendInr);
            }

            base.SetMonthlySalary(monthlySalaryInr);
        }
    }
}
