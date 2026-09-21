namespace EmployeeManagementSystem.Repositories
{
    public readonly struct LegacyBadge
    {
        public object Token { get; }
        public int StaffId { get; }

        public LegacyBadge(object token, int staffId)
        {
            Token = token;
            StaffId = staffId;
        }
    }
}
