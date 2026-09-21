namespace EmployeeManagementSystem.Models
{
    // Value type: a name is just data. Copying it should copy the words, not
    // create a shared "name object" two employees could accidentally mutate.
    public readonly struct PersonName
    {
        public string First { get; }
        public string Last { get; }

        // Middle names are often missing on Indian ID / offer letters.
        public string? Middle { get; }

        public PersonName(string first, string last, string? middle = null)
        {
            First = first;
            Last = last;
            Middle = string.IsNullOrWhiteSpace(middle) ? null : middle;
        }

        public string Full
        {
            get
            {
                if (Middle == null)
                {
                    return $"{First} {Last}";
                }

                return $"{First} {Middle} {Last}";
            }
        }

        public override string ToString()
        {
            return Full;
        }
    }
}
