namespace EmployeeManagementSystem.Models
{
    public class Department
    {
        public int Id { get; }
        public string Name { get; }
        public int CurrentHeadcount { get; private set; }

        public Department(int id, string name)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Department id must be a positive integer.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Department name cannot be empty.", nameof(name));
            }

            Id = id;
            Name = name.Trim();
            CurrentHeadcount = 0;
        }

        internal void Register(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            CurrentHeadcount++;
        }

        public override string ToString()
        {
            return $"{Name} (id {Id}) — {CurrentHeadcount} people";
        }
    }
}
