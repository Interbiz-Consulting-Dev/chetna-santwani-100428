using EmployeeManagementSystem.Enums;
using EmployeeManagementSystem.Interfaces;

namespace EmployeeManagementSystem.Models
{
    // Identity and pay live here. Team roster APIs live in Manager.Team.cs —
    // a light split (the type is not huge yet) so "who this manager is" stays
    // separate from "how they run a team" as that second responsibility grows.
    public partial class Manager : Employee, ITeamCapable
    {
        public Manager(
            PersonName name,
            Address homeAddress,
            DateTime hireDate,
            decimal monthlySalaryInr,
            object statutory)
            : base(name, EmployeeRole.Manager, ContractType.FullTime, homeAddress, hireDate, monthlySalaryInr, statutory)
        {
        }

        internal Manager(EmployeeState state, object statutory)
            : base(state, statutory)
        {
        }

        public override int NoticePeriodDays => 90;

        public override decimal ComputeVariablePay()
        {
            return MonthlySalaryInr * 0.03m * DirectReportCount;
        }

        public override string FormatNotificationLabel()
        {
            return $"{base.FormatNotificationLabel()} (leads {DirectReportCount})";
        }
    }
}
