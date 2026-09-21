using System.Collections;
using EmployeeManagementSystem.Enums;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Notifications;

namespace EmployeeManagementSystem.Repositories
{
    // In-memory employee records: people and departments HR actually queries.
    // Not a generic CRUD wrapper — each store matches an access pattern.
    public sealed class EmployeeRepository
    {
        // Hire order for directories and payroll walks. Scan is O(n); that is
        // fine for "print everyone". It is the wrong structure for "open
        // employee 17" — that is _byStaffId.
        private readonly List<Employee> _hireOrder = new List<Employee>();

        // O(1) staff-id lookup (badge desk, raise, resignation). Duplicate of
        // the List contents, not a second source of truth for identity.
        private readonly Dictionary<int, Employee> _byStaffId = new Dictionary<int, Employee>();

        private readonly Dictionary<int, Department> _departments = new Dictionary<int, Department>();

        // Secondary index: "who sits in Engineering?" without scanning the company.
        private readonly Dictionary<int, List<Employee>> _byDepartment = new Dictionary<int, List<Employee>>();

        // Spec requires a non-generic collection. Reception still keys people by
        // untyped badge tokens from an old turnstile (string today, boxed int
        // tomorrow). Hashtable accepts object keys. Once badges are normalized
        // strings, Dictionary<string, int> is the type I'd actually ship.
        private readonly Hashtable _legacyBadgeToStaffId = new Hashtable();

        // Spec requires a non-generic collection. A 1990s payroll drop still
        // wants an ArrayList of staff ids (ints get boxed). List<int> is the
        // modern equivalent and avoids boxing.
        private readonly ArrayList _legacyPayrollIdBatch = new ArrayList();

        public event StaffLifecycleHandler? StaffHired;
        public event StaffLifecycleHandler? StaffLeft;
        public event StaffAssignedHandler? StaffAssigned;
        public event CompensationChangedHandler? CompensationChanged;

        public void RegisterDepartment(Department department)
        {
            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }

            if (_departments.ContainsKey(department.Id))
            {
                throw new ArgumentException($"Department {department.Id} is already on the books.");
            }

            _departments.Add(department.Id, department);
            _byDepartment.Add(department.Id, new List<Employee>());
        }

        public void Hire(Employee employee, bool announce = true)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            if (_byStaffId.ContainsKey(employee.Id))
            {
                throw new ArgumentException($"Staff id {employee.Id} is already hired.");
            }

            _hireOrder.Add(employee);
            _byStaffId.Add(employee.Id, employee);

            if (announce)
            {
                StaffHired?.Invoke(employee);
            }
        }

        public void AssignToDepartment(Employee employee, Department department, bool announce = true)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }

            if (!_departments.ContainsKey(department.Id))
            {
                throw new ArgumentException($"Department {department.Id} is not registered.");
            }

            employee.AssignTo(department);
            _byDepartment[department.Id].Add(employee);

            if (announce)
            {
                StaffAssigned?.Invoke(employee, department);
            }
        }

        public void RecordRaise(Employee employee, decimal amount, string? reason = null)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            decimal previous = employee.MonthlySalaryInr;
            if (reason == null)
            {
                employee.GiveRaise(amount);
            }
            else
            {
                employee.GiveRaise(amount, reason);
            }

            CompensationChanged?.Invoke(employee, previous, employee.MonthlySalaryInr, employee.LastRaiseReason);
        }

        public void RecordResignation(Employee employee, DateTime lastDay)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            employee.Resign(lastDay);
            StaffLeft?.Invoke(employee);
        }

        // Lightweight pair of related numbers — not worth a PayrollTotals type.
        public (int People, decimal BasePay, decimal VariablePay) SummarizeActivePay()
        {
            List<Employee> active = ListActive();
            decimal basePay = 0m;
            decimal variablePay = 0m;
            for (int i = 0; i < active.Count; i++)
            {
                basePay += active[i].MonthlySalaryInr;
                variablePay += active[i].ComputeVariablePay();
            }

            return (active.Count, basePay, variablePay);
        }

        public Employee? FindByStaffId(int staffId)
        {
            Employee? found;
            if (_byStaffId.TryGetValue(staffId, out found))
            {
                return found;
            }

            return null;
        }

        public Department? FindDepartment(int departmentId)
        {
            Department? found;
            if (_departments.TryGetValue(departmentId, out found))
            {
                return found;
            }

            return null;
        }

        public void RegisterLegacyBadge(object badgeToken, int staffId)
        {
            if (badgeToken == null)
            {
                throw new ArgumentNullException(nameof(badgeToken));
            }

            if (!_byStaffId.ContainsKey(staffId))
            {
                throw new ArgumentException($"Cannot attach a badge to unknown staff id {staffId}.");
            }

            _legacyBadgeToStaffId[badgeToken] = staffId;
        }

        public Employee? FindByLegacyBadge(object badgeToken)
        {
            if (badgeToken == null || !_legacyBadgeToStaffId.ContainsKey(badgeToken))
            {
                return null;
            }

            int staffId = (int)_legacyBadgeToStaffId[badgeToken]!;
            return FindByStaffId(staffId);
        }

        public List<Employee> ListInHireOrder()
        {
            return Copy(_hireOrder);
        }

        public List<Employee> ListActive()
        {
            var result = new List<Employee>();
            for (int i = 0; i < _hireOrder.Count; i++)
            {
                if (_hireOrder[i].Status == EmploymentStatus.Active)
                {
                    result.Add(_hireOrder[i]);
                }
            }

            return result;
        }

        public List<Employee> ListLeavers()
        {
            var result = new List<Employee>();
            for (int i = 0; i < _hireOrder.Count; i++)
            {
                Employee person = _hireOrder[i];
                if (person.Status == EmploymentStatus.Resigned || person.Status == EmploymentStatus.Terminated)
                {
                    result.Add(person);
                }
            }

            return result;
        }

        public List<Employee> ListByDepartment(int departmentId)
        {
            if (!_byDepartment.TryGetValue(departmentId, out List<Employee>? bucket) || bucket == null)
            {
                return new List<Employee>();
            }

            return Copy(bucket);
        }

        public List<Department> ListDepartments()
        {
            var result = new List<Department>();
            foreach (Department department in _departments.Values)
            {
                result.Add(department);
            }

            return result;
        }

        public List<LegacyBadge> ListLegacyBadges()
        {
            var result = new List<LegacyBadge>();
            foreach (DictionaryEntry entry in _legacyBadgeToStaffId)
            {
                if (entry.Key == null || entry.Value == null)
                {
                    continue;
                }

                result.Add(new LegacyBadge(entry.Key, (int)entry.Value));
            }

            return result;
        }

        public ArrayList CopyActiveIdsForLegacyPayrollDrop()
        {
            _legacyPayrollIdBatch.Clear();
            List<Employee> active = ListActive();
            for (int i = 0; i < active.Count; i++)
            {
                _legacyPayrollIdBatch.Add(active[i].Id);
            }

            return _legacyPayrollIdBatch;
        }

        private static List<Employee> Copy(List<Employee> source)
        {
            var copy = new List<Employee>();
            for (int i = 0; i < source.Count; i++)
            {
                copy.Add(source[i]);
            }

            return copy;
        }
    }
}
