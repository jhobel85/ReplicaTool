using System.Timers;
using ReplicaTool.Interfaces;
using ReplicaTool.Common;
using Serilog;

namespace ReplicaTool.Services
{
    public class Scheduler
    {
        private readonly ILogger _log = Logger.CLI_LOGGER;
        
        public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(10);

        public ManualResetEvent ExitEvent = new(false);

        private readonly System.Timers.Timer _timer;

        private readonly IReplicator _target;
        private CancellationTokenSource? _cts;
  
        private int _isReplicationRunning = 0;

        public Scheduler(IReplicator target, TimeSpan? interval = null)
        {
            _target = target;

            var intervalMs = (interval ?? DefaultInterval).TotalMilliseconds;
            _timer = new System.Timers.Timer(intervalMs);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            _cts?.Cancel();
        }

        private async void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            // Atomic check-and-set: Try to change 0 → 1
            // Returns the original value (0 if successful, 1 if already running)
            if (Interlocked.CompareExchange(ref _isReplicationRunning, 1, 0) == 1)
            {
                // Previous replication still running
                _log.Warning("Previous replication still in progress. Skipping this interval. Consider increasing the sync interval.");
                return;
            }

            try
            {
                await _target.ReplicateAsync(_cts?.Token ?? default).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _log.Information("Replication canceled.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unexpected error occured during replication.");
            }
        }

        public void OnExit(object? sender, ConsoleCancelEventArgs e)
        {
            _log.Information("Exiting...");                       
            _timer.Stop();
            _timer.Dispose();            
            _cts?.Cancel();
                        
            // Ensures we read the latest value
            while (Interlocked.CompareExchange(ref _isReplicationRunning, 0, 0) == 1)
            {
                _log.Information("Waiting for current replication to complete...");
                Thread.Sleep(100); // Give other thread a chance to complete.
            }

            _cts?.Dispose();
            e.Cancel = true;
            ExitEvent.Set();
            _log.Information("Scheduler stopped and resources cleaned up.");
        }

    }
}