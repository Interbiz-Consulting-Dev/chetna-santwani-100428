namespace EmployeeManagementSystem.Models
{
    public class Department
    {
        public int Id { get; }
        public string Name { get; }
        public int CurrentHeadcount { get; private set; }

        public Department(int id, string name)
        {
            Id = id;
            Name = name;
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
