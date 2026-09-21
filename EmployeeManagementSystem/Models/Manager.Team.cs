namespace EmployeeManagementSystem.Models
{
    public partial class Manager
    {
        private readonly string[] _directReports = new string[32];
        private int _directReportCount;

        public int DirectReportCount => _directReportCount;

        public string[] CopyDirectReportNames()
        {
            var names = new string[_directReportCount];
            for (int i = 0; i < _directReportCount; i++)
            {
                names[i] = _directReports[i];
            }

            return names;
        }

        public void AddDirectReport(string employeeName)
        {
            if (string.IsNullOrWhiteSpace(employeeName))
            {
                throw new ArgumentException("Direct report name is required.", nameof(employeeName));
            }

            if (_directReportCount >= _directReports.Length)
            {
                throw new InvalidOperationException("This manager's roster is full.");
            }

            _directReports[_directReportCount] = employeeName;
            _directReportCount++;
        }
    }
}
