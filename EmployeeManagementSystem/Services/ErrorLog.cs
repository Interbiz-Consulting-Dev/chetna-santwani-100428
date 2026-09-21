using System.Text;

namespace EmployeeManagementSystem.Services
{
    public sealed class ErrorLog
    {
        private readonly string _logPath;
        private readonly SemaphoreSlim _writeGate = new SemaphoreSlim(1, 1);

        public ErrorLog(string logPath)
        {
            _logPath = logPath;
            string? folder = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public void Record(string operation, Exception exception, string context)
        {
            string entry = BuildEntry(operation, exception, context);

            _writeGate.Wait();
            try
            {
                File.AppendAllText(_logPath, entry, Encoding.UTF8);
            }
            catch (IOException logFailure)
            {
                Console.WriteLine($"Error log unavailable: {logFailure.Message}");
            }
            catch (UnauthorizedAccessException logFailure)
            {
                Console.WriteLine($"Error log unavailable: {logFailure.Message}");
            }
            finally
            {
                _writeGate.Release();
            }
        }

        public async Task RecordAsync(
            string operation,
            Exception exception,
            string context,
            CancellationToken cancellationToken = default)
        {
            string entry = BuildEntry(operation, exception, context);

            await _writeGate.WaitAsync(cancellationToken);
            try
            {
                await File.AppendAllTextAsync(_logPath, entry, Encoding.UTF8, cancellationToken);
            }
            catch (IOException logFailure)
            {
                Console.WriteLine($"Error log unavailable: {logFailure.Message}");
            }
            catch (UnauthorizedAccessException logFailure)
            {
                Console.WriteLine($"Error log unavailable: {logFailure.Message}");
            }
            finally
            {
                _writeGate.Release();
            }
        }

        public async Task<List<string>> ReadRecentAsync(
            int maxEntries,
            CancellationToken cancellationToken = default)
        {
            var entries = new List<string>();
            if (maxEntries <= 0 || !File.Exists(_logPath))
            {
                return entries;
            }

            await _writeGate.WaitAsync(cancellationToken);
            try
            {
                string[] lines = await File.ReadAllLinesAsync(_logPath, Encoding.UTF8, cancellationToken);
                var current = new StringBuilder();
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == "-----" && current.Length > 0)
                    {
                        entries.Add(current.ToString());
                        current.Clear();
                    }

                    if (current.Length > 0)
                    {
                        current.AppendLine();
                    }

                    current.Append(lines[i]);
                }

                if (current.Length > 0)
                {
                    entries.Add(current.ToString());
                }
            }
            finally
            {
                _writeGate.Release();
            }

            int first = entries.Count - maxEntries;
            if (first < 0)
            {
                first = 0;
            }

            var recent = new List<string>();
            for (int i = first; i < entries.Count; i++)
            {
                recent.Add(entries[i]);
            }

            return recent;
        }

        private static string BuildEntry(string operation, Exception exception, string context)
        {
            return
                "-----" + Environment.NewLine +
                "When (UTC): " + DateTime.UtcNow.ToString("O") + Environment.NewLine +
                "Operation: " + operation + Environment.NewLine +
                "Exception: " + exception.GetType().FullName + Environment.NewLine +
                "Message: " + exception.Message + Environment.NewLine +
                "Context: " + context + Environment.NewLine +
                "Stack: " + exception.StackTrace + Environment.NewLine;
        }

        public string LogPath => _logPath;
    }
}
