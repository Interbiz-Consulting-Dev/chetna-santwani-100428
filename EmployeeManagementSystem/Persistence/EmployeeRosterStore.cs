using System.Globalization;
using System.Text;
using EmployeeManagementSystem.Enums;
using EmployeeManagementSystem.Exceptions;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Repositories;
using EmployeeManagementSystem.Services;

namespace EmployeeManagementSystem.Persistence
{
    // Owns an exclusive lock FileStream for the roster file for the lifetime
    // of the store (save then load in one session). IDisposable is the right
    // tool: the OS handle must be released deterministically, not whenever GC
    // happens to run. Callers should wrap this in `using`.
    //
    // No finalizer on this type. We only hold a managed FileStream. That type
    // already finalizes its own handle if Dispose is skipped. A finalizer here
    // would be the IntPtr/unmanaged-handle pattern, which we do not have.
    public sealed class EmployeeFileStorage : IDisposable
    {
        private readonly string _filePath;
        private readonly string _lockPath;
        private readonly ErrorLog _errorLog;
        private FileStream? _lockStream;
        private bool _disposed;

        public EmployeeFileStorage(string filePath, ErrorLog? errorLog = null)
        {
            _filePath = filePath;
            _lockPath = filePath + ".lock";

            string? folder = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            _errorLog = errorLog ?? new ErrorLog(
                Path.Combine(folder ?? AppContext.BaseDirectory, "errors.log"));

            _lockStream = new FileStream(
                _lockPath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);
        }

        public (int DepartmentCount, int PersonCount, int BadgeCount) Save(EmployeeRepository workforce)
        {
            return SaveAsync(workforce).GetAwaiter().GetResult();
        }

        public async Task<(int DepartmentCount, int PersonCount, int BadgeCount)> SaveAsync(
            EmployeeRepository workforce,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            int departmentCount = workforce.ListDepartments().Count;
            int personCount = workforce.ListInHireOrder().Count;
            int badgeCount = workforce.ListLegacyBadges().Count;

            string tempPath = _filePath + ".tmp";
            try
            {
                string rosterText = BuildRosterText(workforce);
                await File.WriteAllTextAsync(tempPath, rosterText, Encoding.UTF8, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                File.Copy(tempPath, _filePath, true);
                File.Delete(tempPath);
                return (departmentCount, personCount, badgeCount);
            }
            catch (OperationCanceledException)
            {
                TryDelete(tempPath);
                throw;
            }
            catch (IOException ex)
            {
                await _errorLog.RecordAsync("Employee file save", ex, $"Path={_filePath}", cancellationToken);
                Console.WriteLine($"Could not save roster: {ex.Message}");
                TryDelete(tempPath);
                return (0, 0, 0);
            }
        }

        public EmployeeRepository Load()
        {
            return LoadAsync().GetAwaiter().GetResult();
        }

        public async Task<EmployeeRepository> LoadAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Employee.ResetCensusForFileLoad();
            var workforce = new EmployeeRepository();

            if (!File.Exists(_filePath))
            {
                Console.WriteLine("No employee file on disk yet — empty books.");
                return workforce;
            }

            string[] lines;
            try
            {
                lines = await File.ReadAllLinesAsync(_filePath, Encoding.UTF8, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (IOException ex)
            {
                await _errorLog.RecordAsync("Employee file load", ex, $"Path={_filePath}", cancellationToken);
                Console.WriteLine($"Could not read roster: {ex.Message}");
                return workforce;
            }

            if (lines.Length == 0)
            {
                Console.WriteLine("Employee file is empty — empty books.");
                return workforce;
            }

            var pendingPeople = new List<PendingPerson>();
            for (int i = 0; i < lines.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line) || line[0] == '#')
                {
                    continue;
                }

                try
                {
                    ApplyLine(line, i + 1, workforce, pendingPeople);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    RosterFormatException formatException = ex as RosterFormatException
                        ?? new RosterFormatException(i + 1, ex.Message, ex);
                    await _errorLog.RecordAsync(
                        "Roster row validation",
                        formatException,
                        $"Path={_filePath}; Line={i + 1}; Content={line}",
                        cancellationToken);
                    Console.WriteLine($"Skipping malformed roster line {i + 1}: {formatException.Message}");
                }
            }

            for (int i = 0; i < pendingPeople.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    AttachPerson(workforce, pendingPeople[i]);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    await _errorLog.RecordAsync(
                        "Roster record restore",
                        ex,
                        $"Path={_filePath}; Pending record index={i}",
                        cancellationToken);
                    Console.WriteLine($"Skipping roster record: {ex.Message}");
                }
            }

            return workforce;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _lockStream?.Dispose();
            _lockStream = null;
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EmployeeFileStorage));
            }
        }

        private static string BuildRosterText(EmployeeRepository workforce)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                writer.WriteLine("# Northwind roster v1");
                WriteDepartments(writer, workforce);
                WritePeople(writer, workforce);
                WriteBadges(writer, workforce);
                return writer.ToString();
            }
        }

        private static void WriteDepartments(TextWriter writer, EmployeeRepository workforce)
        {
            List<Department> departments = workforce.ListDepartments();
            for (int i = 0; i < departments.Count; i++)
            {
                Department d = departments[i];
                writer.WriteLine("DEPT\t" + d.Id + "\t" + Sanitize(d.Name));
            }
        }

        private static void WritePeople(TextWriter writer, EmployeeRepository workforce)
        {
            List<Employee> people = workforce.ListInHireOrder();
            for (int i = 0; i < people.Count; i++)
            {
                Employee person = people[i];
                string lastDay = person.LastDay.HasValue
                    ? person.LastDay.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : string.Empty;
                string dept = person.DepartmentId.HasValue
                    ? person.DepartmentId.Value.ToString(CultureInfo.InvariantCulture)
                    : string.Empty;

                writer.WriteLine(
                    "PERSON\t" +
                    KindOf(person) + "\t" +
                    person.Id + "\t" +
                    Sanitize(person.Name.First) + "\t" +
                    Sanitize(person.Name.Middle ?? string.Empty) + "\t" +
                    Sanitize(person.Name.Last) + "\t" +
                    person.Role + "\t" +
                    person.Contract + "\t" +
                    person.Status + "\t" +
                    person.HireDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "\t" +
                    person.MonthlySalaryInr.ToString(CultureInfo.InvariantCulture) + "\t" +
                    Sanitize(person.LastRaiseReason) + "\t" +
                    lastDay + "\t" +
                    Sanitize(person.HomeAddress.Street) + "\t" +
                    Sanitize(person.HomeAddress.City) + "\t" +
                    Sanitize(person.HomeAddress.State) + "\t" +
                    Sanitize(person.HomeAddress.PostalCode) + "\t" +
                    dept + "\t" +
                    Sanitize(EncodeStatutory(person)) + "\t" +
                    Sanitize(EncodeExtra(person)));
            }
        }

        private static void WriteBadges(TextWriter writer, EmployeeRepository workforce)
        {
            List<LegacyBadge> badges = workforce.ListLegacyBadges();
            for (int i = 0; i < badges.Count; i++)
            {
                LegacyBadge badge = badges[i];
                if (badge.Token is int numeric)
                {
                    writer.WriteLine("BADGE\tINT\t" + numeric + "\t" + badge.StaffId);
                }
                else
                {
                    writer.WriteLine("BADGE\tSTR\t" + Sanitize(badge.Token.ToString() ?? string.Empty) + "\t" + badge.StaffId);
                }
            }
        }

        private static void ApplyLine(string line, int lineNumber, EmployeeRepository workforce, List<PendingPerson> pendingPeople)
        {
            string[] parts = line.Split('\t');
            if (parts.Length == 0)
            {
                throw new FormatException("empty record");
            }

            if (parts[0] == "DEPT")
            {
                if (parts.Length < 3)
                {
                    throw new FormatException("department needs id and name");
                }

                int id = int.Parse(parts[1], CultureInfo.InvariantCulture);
                workforce.RegisterDepartment(new Department(id, parts[2]));
                return;
            }

            if (parts[0] == "PERSON")
            {
                if (parts.Length < 20)
                {
                    throw new FormatException("person record is short");
                }

                pendingPeople.Add(ParsePerson(parts));
                return;
            }

            if (parts[0] == "BADGE")
            {
                if (parts.Length < 4)
                {
                    throw new FormatException("badge record is short");
                }

                pendingPeople.Add(new PendingPerson { BadgeOnly = true, BadgeKind = parts[1], BadgeToken = parts[2], BadgeStaffId = int.Parse(parts[3], CultureInfo.InvariantCulture) });
                return;
            }

            throw new FormatException("unknown record type '" + parts[0] + "' on line " + lineNumber);
        }

        private static PendingPerson ParsePerson(string[] parts)
        {
            DateTime? lastDay = null;
            if (!string.IsNullOrWhiteSpace(parts[12]))
            {
                lastDay = DateTime.ParseExact(parts[12], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            int? departmentId = null;
            if (!string.IsNullOrWhiteSpace(parts[17]))
            {
                departmentId = int.Parse(parts[17], CultureInfo.InvariantCulture);
            }

            var state = new EmployeeState
            {
                Id = int.Parse(parts[2], CultureInfo.InvariantCulture),
                Name = new PersonName(parts[3], parts[5], string.IsNullOrWhiteSpace(parts[4]) ? null : parts[4]),
                Role = Enum.Parse<EmployeeRole>(parts[6]),
                Contract = Enum.Parse<ContractType>(parts[7]),
                Status = Enum.Parse<EmploymentStatus>(parts[8]),
                HireDate = DateTime.ParseExact(parts[9], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                MonthlySalaryInr = decimal.Parse(parts[10], CultureInfo.InvariantCulture),
                LastRaiseReason = parts[11],
                LastDay = lastDay,
                HomeAddress = new Address(parts[13], parts[14], parts[15], parts[16]),
                DepartmentId = departmentId,
                Extra = parts[19]
            };

            return new PendingPerson
            {
                Kind = parts[1],
                State = state,
                StatutoryBlob = parts[18]
            };
        }

        private static void AttachPerson(EmployeeRepository workforce, PendingPerson pending)
        {
            if (pending.BadgeOnly)
            {
                object token = pending.BadgeKind == "INT"
                    ? int.Parse(pending.BadgeToken, CultureInfo.InvariantCulture)
                    : (object)pending.BadgeToken;
                workforce.RegisterLegacyBadge(token, pending.BadgeStaffId);
                return;
            }

            Employee person = CreateSubtype(pending.Kind, pending.State, DecodeStatutory(pending.Kind, pending.StatutoryBlob));
            workforce.Hire(person, announce: false);

            if (pending.State.DepartmentId.HasValue)
            {
                Department? department = workforce.FindDepartment(pending.State.DepartmentId.Value);
                if (department == null)
                {
                    Console.WriteLine($"Staff {person.Id} refers to missing department {pending.State.DepartmentId.Value}.");
                }
                else
                {
                    workforce.AssignToDepartment(person, department, announce: false);
                }
            }

            if (person is Manager manager && !string.IsNullOrWhiteSpace(pending.State.Extra))
            {
                string[] names = pending.State.Extra.Split(';');
                for (int i = 0; i < names.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(names[i]))
                    {
                        manager.AddDirectReport(names[i]);
                    }
                }
            }
        }

        private static Employee CreateSubtype(string kind, EmployeeState state, object statutory)
        {
            switch (kind)
            {
                case "Developer":
                    return new Developer(state, statutory);
                case "Manager":
                    return new Manager(state, statutory);
                case "Intern":
                    return new Intern(state, statutory);
                case "Contractor":
                    return new Contractor(state, statutory);
                default:
                    throw new FormatException("unknown employee kind '" + kind + "'");
            }
        }

        private static string KindOf(Employee person)
        {
            if (person is Developer)
            {
                return "Developer";
            }

            if (person is Manager)
            {
                return "Manager";
            }

            if (person is Intern)
            {
                return "Intern";
            }

            if (person is Contractor)
            {
                return "Contractor";
            }

            throw new InvalidOperationException("Cannot persist an unknown employee type.");
        }

        private static string EncodeExtra(Employee person)
        {
            Developer? developer = person as Developer;
            if (developer != null)
            {
                return developer.PrimarySkill;
            }

            Manager? manager = person as Manager;
            if (manager != null)
            {
                string[] names = manager.CopyDirectReportNames();
                var joined = new StringBuilder();
                for (int i = 0; i < names.Length; i++)
                {
                    if (i > 0)
                    {
                        joined.Append(';');
                    }

                    joined.Append(names[i]);
                }

                return joined.ToString();
            }

            return string.Empty;
        }

        private static string EncodeStatutory(Employee person)
        {
            if (person is Contractor)
            {
                return "GSTIN=" + (string)person.Statutory.Gstin;
            }

            return "PAN=" + (string)person.Statutory.Pan + ";UAN=" + (string)person.Statutory.Uan;
        }

        private static object DecodeStatutory(string kind, string blob)
        {
            if (kind == "Contractor")
            {
                return new { Gstin = ValueAfter(blob, "GSTIN=") };
            }

            string pan = string.Empty;
            string uan = string.Empty;
            string[] bits = blob.Split(';');
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i].StartsWith("PAN=", StringComparison.Ordinal))
                {
                    pan = bits[i].Substring(4);
                }
                else if (bits[i].StartsWith("UAN=", StringComparison.Ordinal))
                {
                    uan = bits[i].Substring(4);
                }
            }

            return new { Pan = pan, Uan = uan };
        }

        private static string ValueAfter(string blob, string prefix)
        {
            if (blob.StartsWith(prefix, StringComparison.Ordinal))
            {
                return blob.Substring(prefix.Length);
            }

            return blob;
        }

        private static string Sanitize(string value)
        {
            return value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
            }
        }

        private sealed class PendingPerson
        {
            public bool BadgeOnly;
            public string Kind = string.Empty;
            public EmployeeState State = new EmployeeState();
            public string StatutoryBlob = string.Empty;
            public string BadgeKind = string.Empty;
            public string BadgeToken = string.Empty;
            public int BadgeStaffId;
        }
    }
}
