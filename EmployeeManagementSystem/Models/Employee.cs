using EmployeeManagementSystem.Enums;
using EmployeeManagementSystem.Exceptions;
using EmployeeManagementSystem.Policies;

namespace EmployeeManagementSystem.Models
{
    // Abstract class = what someone IS: a person on this company's books.
    // There is no generic hire — payroll, notice, and variable pay only make
    // sense on a concrete contract (full-time developer, manager, intern,
    // contractor). Interface ITeamCapable is the CAN-DO counterpart for
    // leading a team; it is not on this type because most employees do not.
    public abstract class Employee
    {
        private static int _nextStaffId = 1;
        private static int _activeHeadcount;

        private decimal _monthlySalaryInr;
        private string _lastRaiseReason = "none";

        public int Id { get; }
        public PersonName Name { get; }
        public EmployeeRole Role { get; }
        public ContractType Contract { get; }
        public EmploymentStatus Status { get; private set; }
        public Address HomeAddress { get; private set; }
        public DateTime HireDate { get; }

        public decimal MonthlySalaryInr => _monthlySalaryInr;

        public string LastRaiseReason => _lastRaiseReason;

        public DateTime? LastDay { get; private set; }
        public DateTime? ProbationEndsOn { get; }
        public int? DepartmentId { get; private set; }

        // Unchanged from Phase 1: vendor statutory payloads still do not share
        // a schema (PAN/UAN vs GSTIN). dynamic stays until typed profiles exist.
        public dynamic Statutory { get; }

        protected Employee(
            PersonName name,
            EmployeeRole role,
            ContractType contract,
            Address homeAddress,
            DateTime hireDate,
            decimal monthlySalaryInr,
            object statutory)
        {
            Id = AllocateStaffId();
            Name = name;
            Role = role;
            Contract = contract;
            Status = EmploymentStatus.Active;
            HomeAddress = homeAddress;
            HireDate = hireDate;
            LastDay = null;
            ProbationEndsOn = CompanyRules.ComputeProbationEnd(contract, hireDate);
            DepartmentId = null;
            Statutory = statutory;
            SetMonthlySalary(monthlySalaryInr);
            _activeHeadcount++;
        }

        public static int AllocateStaffId()
        {
            int id = _nextStaffId;
            _nextStaffId++;
            return id;
        }

        public static int ActiveHeadcount => _activeHeadcount;

        // Load rebuilds the census from disk. These counters are process-wide
        // (Phase 1), so a reload must rewind them or ids/headcount double.
        internal static void ResetCensusForFileLoad()
        {
            _nextStaffId = 1;
            _activeHeadcount = 0;
        }

        // Rehydrate from disk: keep the saved id/status/salary. Do not go through
        // AllocateStaffId or Resign() — those are hire/exit workflows, not restore.
        protected Employee(EmployeeState state, object statutory)
        {
            Id = state.Id;
            Name = state.Name;
            Role = state.Role;
            Contract = state.Contract;
            Status = state.Status;
            HomeAddress = state.HomeAddress;
            HireDate = state.HireDate;
            _monthlySalaryInr = state.MonthlySalaryInr;
            _lastRaiseReason = state.LastRaiseReason;
            LastDay = state.LastDay;
            ProbationEndsOn = CompanyRules.ComputeProbationEnd(state.Contract, state.HireDate);
            DepartmentId = null;
            Statutory = statutory;

            if (state.Id >= _nextStaffId)
            {
                _nextStaffId = state.Id + 1;
            }

            if (Status == EmploymentStatus.Active)
            {
                _activeHeadcount++;
            }
        }

        // Encapsulation: every salary change (hire, raise) goes through here so
        // payroll cannot store a figure below the company floor or on a leaver.
        protected virtual void SetMonthlySalary(decimal monthlySalaryInr)
        {
            if (Status == EmploymentStatus.Resigned || Status == EmploymentStatus.Terminated)
            {
                throw new EmploymentEndedException(Id);
            }

            if (!CompanyRules.MeetsSalaryFloor(monthlySalaryInr))
            {
                throw new SalaryFloorViolationException(
                    monthlySalaryInr,
                    CompanyRules.MinimumMonthlySalaryInr);
            }

            _monthlySalaryInr = monthlySalaryInr;
        }

        public void AssignTo(Department department)
        {
            DepartmentId = department.Id;
            department.Register(this);
        }

        public Address ProposeRelocation(string street, string city, string state, string postalCode)
        {
            return HomeAddress.RelocateTo(street, city, state, postalCode);
        }

        public void ConfirmRelocation(Address confirmed)
        {
            HomeAddress = confirmed;
        }

        public bool IsOnProbation(DateTime asOf)
        {
            if (!ProbationEndsOn.HasValue || Status != EmploymentStatus.Active)
            {
                return false;
            }

            return asOf.Date <= ProbationEndsOn.Value.Date;
        }

        public void Resign(DateTime lastDay)
        {
            if (Status == EmploymentStatus.Resigned || Status == EmploymentStatus.Terminated)
            {
                throw new AlreadyExitedException(Id);
            }

            Status = EmploymentStatus.Resigned;
            LastDay = lastDay;
            _activeHeadcount--;
        }

        public void GiveRaise(decimal amount)
        {
            GiveRaise(amount, "Unspecified");
        }

        public void GiveRaise(decimal amount, string reason)
        {
            if (amount <= 0)
            {
                throw new InvalidRaiseException(amount);
            }

            SetMonthlySalary(_monthlySalaryInr + amount);
            _lastRaiseReason = string.IsNullOrWhiteSpace(reason) ? "Unspecified" : reason;
        }

        // Overriding, not a new method per role: payroll always asks "what is
        // this person's variable pay this month?" The formula is the thing that
        // changes. A separate ComputeDeveloperBonus() would force callers to
        // branch on type and miss people.
        public abstract decimal ComputeVariablePay();

        public abstract int NoticePeriodDays { get; }

        public virtual string DescribeStatutory()
        {
            return $"PAN {Statutory.Pan}, UAN {Statutory.Uan}";
        }

        // Default audit label is enough for most roles. Subtypes override when
        // people-ops needs extra context (team size, intern track) without the
        // notification layer switching on runtime type.
        public virtual string FormatNotificationLabel()
        {
            return $"{Name} [{Id}] {Role}";
        }

        public string DescribeCompensation()
        {
            return $"{Name}: {MonthlySalaryInr:C} base + {ComputeVariablePay():C} variable, {NoticePeriodDays}-day notice";
        }

        public string GetEmploymentSummary(DateTime asOf)
        {
            if (LastDay.HasValue)
            {
                return $"{Name} [{Id}] — {Status} on {LastDay.Value.ToShortDateString()} (hired {HireDate.ToShortDateString()}, {NoticePeriodDays}-day notice)";
            }

            if (IsOnProbation(asOf) && ProbationEndsOn.HasValue)
            {
                return $"{Name} [{Id}] — {Role}, probation until {ProbationEndsOn.Value.ToShortDateString()}";
            }

            return $"{Name} [{Id}] — {Role}, {Status} since {HireDate.ToShortDateString()}";
        }

        public override string ToString()
        {
            string dept = DepartmentId.HasValue ? $"dept {DepartmentId.Value}" : "unassigned";
            return $"[{Id}] {Name} | {Role} | {Contract} | {Status} | {HomeAddress} | {dept} | {MonthlySalaryInr:C}/mo";
        }
    }
}
