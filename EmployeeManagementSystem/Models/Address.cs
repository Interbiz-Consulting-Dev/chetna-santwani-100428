namespace EmployeeManagementSystem.Models
{
    // Value type: an address has no identity of its own. Two records with the
    // same street/city/pin are the same address. Assignment copies fields, so
    // HR can preview a move without touching the on-file address (see Employee
    // ProposeRelocation). A class would share one object and a "preview" would
    // mutate production data.
    //
    // readonly so callers cannot mutate a copy and think they edited the
    // employee's stored address; they must replace the whole value.
    public readonly struct Address
    {
        public string Street { get; }
        public string City { get; }
        public string State { get; }
        public string PostalCode { get; }

        public Address(string street, string city, string state, string postalCode)
        {
            Street = street;
            City = city;
            State = state;
            PostalCode = postalCode;
        }

        public Address RelocateTo(string street, string city, string state, string postalCode)
        {
            return new Address(street, city, state, postalCode);
        }

        public override string ToString()
        {
            return $"{Street}, {City}, {State} {PostalCode}";
        }
    }
}
