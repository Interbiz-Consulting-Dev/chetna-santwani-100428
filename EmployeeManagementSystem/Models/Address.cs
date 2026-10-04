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
            if (string.IsNullOrWhiteSpace(street))
            {
                throw new ArgumentException("Street cannot be empty.", nameof(street));
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                throw new ArgumentException("City cannot be empty.", nameof(city));
            }

            if (ContainsDigits(city))
            {
                throw new ArgumentException($"City '{city}' cannot contain numbers.", nameof(city));
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentException("State cannot be empty.", nameof(state));
            }

            if (ContainsDigits(state))
            {
                throw new ArgumentException($"State '{state}' cannot contain numbers.", nameof(state));
            }

            if (string.IsNullOrWhiteSpace(postalCode))
            {
                throw new ArgumentException("Postal code cannot be empty.", nameof(postalCode));
            }

            Street = street.Trim();
            City = city.Trim();
            State = state.Trim();
            PostalCode = postalCode.Trim();
        }

        public static bool ContainsDigits(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsDigit(text[i]))
                {
                    return true;
                }
            }

            return false;
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
