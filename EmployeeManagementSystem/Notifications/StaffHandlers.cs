using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Notifications
{
    // Named delegate: the roster publishes "a person joined/left". It does not
    // know whether audit, census, or a probation watch is listening.
    public delegate void StaffLifecycleHandler(Employee person);

    public delegate void StaffAssignedHandler(Employee person, Department department);

    public delegate void CompensationChangedHandler(
        Employee person,
        decimal previousPay,
        decimal currentPay,
        string reason);
}
