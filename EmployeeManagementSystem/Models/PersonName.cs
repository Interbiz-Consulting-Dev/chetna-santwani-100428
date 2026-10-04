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
            if (string.IsNullOrWhiteSpace(first))
            {
                throw new ArgumentException("First name cannot be empty.", nameof(first));
            }

            if (ContainsDigits(first))
            {
                throw new ArgumentException($"First name '{first}' cannot contain numbers.", nameof(first));
            }

            if (!IsValidName(first))
            {
                throw new ArgumentException($"First name '{first}' contains invalid characters.", nameof(first));
            }

            if (string.IsNullOrWhiteSpace(last))
            {
                throw new ArgumentException("Last name cannot be empty.", nameof(last));
            }

            if (ContainsDigits(last))
            {
                throw new ArgumentException($"Last name '{last}' cannot contain numbers.", nameof(last));
            }

            if (!IsValidName(last))
            {
                throw new ArgumentException($"Last name '{last}' contains invalid characters.", nameof(last));
            }

            string? trimmedMiddle = string.IsNullOrWhiteSpace(middle) ? null : middle.Trim();
            if (trimmedMiddle != null)
            {
                if (ContainsDigits(trimmedMiddle))
                {
                    throw new ArgumentException($"Middle name '{middle}' cannot contain numbers.", nameof(middle));
                }

                if (!IsValidName(trimmedMiddle))
                {
                    throw new ArgumentException($"Middle name '{middle}' contains invalid characters.", nameof(middle));
                }
            }

            First = first.Trim();
            Last = last.Trim();
            Middle = trimmedMiddle;
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

        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            bool hasLetter = false;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsLetter(c))
                {
                    hasLetter = true;
                }
                else if (c != ' ' && c != '-' && c != '\'' && c != '.')
                {
                    return false;
                }
            }

            return hasLetter;
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
