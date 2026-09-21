namespace EmployeeManagementSystem.Exceptions
{
    public class EmsException : Exception
    {
        public EmsException(string message)
            : base(message)
        {
        }

        public EmsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public sealed class SalaryFloorViolationException : EmsException
    {
        public SalaryFloorViolationException(decimal salary, decimal minimum)
            : base($"Monthly salary {salary:C} is below the company floor of {minimum:C}.")
        {
            Salary = salary;
            Minimum = minimum;
        }

        public decimal Salary { get; }
        public decimal Minimum { get; }
    }

    public sealed class InternStipendLimitException : EmsException
    {
        public InternStipendLimitException(decimal stipend, decimal maximum)
            : base($"Intern stipend {stipend:C} exceeds the maximum of {maximum:C}.")
        {
            Stipend = stipend;
            Maximum = maximum;
        }

        public decimal Stipend { get; }
        public decimal Maximum { get; }
    }

    public sealed class InvalidRaiseException : EmsException
    {
        public InvalidRaiseException(decimal amount)
            : base($"Raise amount {amount:C} must be greater than zero.")
        {
            Amount = amount;
        }

        public decimal Amount { get; }
    }

    public sealed class EmploymentEndedException : EmsException
    {
        public EmploymentEndedException(int staffId)
            : base($"Salary cannot change because staff member {staffId} has left the company.")
        {
            StaffId = staffId;
        }

        public int StaffId { get; }
    }

    public sealed class AlreadyExitedException : EmsException
    {
        public AlreadyExitedException(int staffId)
            : base($"Staff member {staffId} has already left the company.")
        {
            StaffId = staffId;
        }

        public int StaffId { get; }
    }

    public sealed class RosterFormatException : EmsException
    {
        public RosterFormatException(int lineNumber, string reason)
            : base($"Roster line {lineNumber} is invalid: {reason}")
        {
            LineNumber = lineNumber;
        }

        public RosterFormatException(int lineNumber, string reason, Exception innerException)
            : base($"Roster line {lineNumber} is invalid: {reason}", innerException)
        {
            LineNumber = lineNumber;
        }

        public int LineNumber { get; }
    }
}
