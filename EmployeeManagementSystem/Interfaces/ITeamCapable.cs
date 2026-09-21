namespace EmployeeManagementSystem.Interfaces
{
    // Capability, not identity: leading a team is something some employees CAN
    // DO. A Manager is still an Employee (abstract class). An Intern is also an
    // Employee but must not be forced to implement team APIs. A future TeamLead
    // could take this interface without becoming a Manager in the class tree.
    public interface ITeamCapable
    {
        void AddDirectReport(string employeeName);
        int DirectReportCount { get; }
    }
}
