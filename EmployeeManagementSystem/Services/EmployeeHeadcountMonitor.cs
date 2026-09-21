using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Services
{
    public sealed class EmployeeHeadcountMonitor
    {
        private readonly CancellationToken _cancellationToken;
        private readonly TimeSpan _interval;
        private Thread? _thread;
        private int _sampleCount;
        private int _lastObservedHeadcount;

        public EmployeeHeadcountMonitor(CancellationToken cancellationToken, TimeSpan interval)
        {
            _cancellationToken = cancellationToken;
            _interval = interval;
        }

        public int SampleCount => Volatile.Read(ref _sampleCount);

        public int LastObservedHeadcount => Volatile.Read(ref _lastObservedHeadcount);

        public void Start()
        {
            if (_thread != null)
            {
                throw new InvalidOperationException("The headcount monitor has already started.");
            }

            _thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "EMS headcount monitor"
            };
            _thread.Start();
        }

        public void Join()
        {
            _thread?.Join();
        }

        private void Run()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                Interlocked.Exchange(ref _lastObservedHeadcount, Employee.ActiveHeadcount);
                Interlocked.Increment(ref _sampleCount);
                Thread.Sleep(_interval);
            }
        }
    }
}
