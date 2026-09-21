using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Repositories;

namespace EmployeeManagementSystem.Notifications
{
    // Independent reactions to roster changes. EmployeeRepository only raises events;
    // this type can be replaced without touching Hire/Resign.
    public sealed class EmployeeNotificationService
    {
        private readonly DateTime _asOf;

        public int HiresThisSession { get; private set; }
        public int ExitsThisSession { get; private set; }
        public int RaisesThisSession { get; private set; }
        public int ProbationWatch { get; private set; }
        public bool PayrollNeedsRefresh { get; private set; }

        public EmployeeNotificationService(DateTime asOf)
        {
            _asOf = asOf;
        }

        public void Attach(EmployeeRepository employees)
        {
            // Multicast: one hire must hit the audit trail AND the session census.
            // Both are people-ops responses to the same fact, not unrelated demos.
            employees.StaffHired += WriteHireAudit;
            employees.StaffHired += CountHire;
            employees.StaffHired += person =>
            {
                if (person.IsOnProbation(_asOf))
                {
                    ProbationWatch++;
                    Console.WriteLine($"  [ops] probation watch: {person.FormatNotificationLabel()} until {person.ProbationEndsOn:yyyy-MM-dd}");
                }
            };

            employees.StaffAssigned += (person, department) =>
            {
                Console.WriteLine($"  [ops] assigned {person.FormatNotificationLabel()} → {department.Name}");
            };

            employees.CompensationChanged += WriteRaiseAudit;
            employees.CompensationChanged += MarkPayrollDirty;

            employees.StaffLeft += WriteExitAudit;
            employees.StaffLeft += CountExit;
        }

        public void WriteSessionSummary()
        {
            Console.WriteLine(
                $"Session: {HiresThisSession} hires, {ExitsThisSession} exits, {RaisesThisSession} raises, {ProbationWatch} on probation watch, payroll dirty={PayrollNeedsRefresh}");
        }

        private void WriteHireAudit(Employee person)
        {
            Console.WriteLine($"  [ops] hired {person.FormatNotificationLabel()}");
        }

        private void CountHire(Employee person)
        {
            HiresThisSession++;
        }

        private void WriteRaiseAudit(Employee person, decimal previousPay, decimal currentPay, string reason)
        {
            RaisesThisSession++;
            Console.WriteLine(
                $"  [ops] pay {person.FormatNotificationLabel()}: {previousPay:C} → {currentPay:C} ({reason})");
        }

        private void MarkPayrollDirty(Employee person, decimal previousPay, decimal currentPay, string reason)
        {
            PayrollNeedsRefresh = true;
        }

        private void WriteExitAudit(Employee person)
        {
            Console.WriteLine($"  [ops] exit {person.FormatNotificationLabel()} last day {person.LastDay:yyyy-MM-dd}");
        }

        private void CountExit(Employee person)
        {
            ExitsThisSession++;
        }
    }
}
