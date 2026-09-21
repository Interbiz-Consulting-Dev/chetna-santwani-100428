using EmployeeManagementSystem.Enums;

namespace EmployeeManagementSystem.Models
{
    // Snapshot used only when reading a roster file. Not an HR workflow type.
    public sealed class EmployeeState
    {
        public int Id { get; set; }
        public PersonName Name { get; set; }
        public EmployeeRole Role { get; set; }
        public ContractType Contract { get; set; }
        public EmploymentStatus Status { get; set; }
        public Address HomeAddress { get; set; }
        public DateTime HireDate { get; set; }
        public decimal MonthlySalaryInr { get; set; }
        public string LastRaiseReason { get; set; } = "none";
        public DateTime? LastDay { get; set; }
        public int? DepartmentId { get; set; }
        public string Extra { get; set; } = string.Empty;
    }
}
