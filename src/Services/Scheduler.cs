using System;
using System.Timers;
using ReplicaTool.Interfaces;

namespace ReplicaTool.Services
{
    public class Scheduler
    {
        public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(10);

        private readonly System.Timers.Timer _timer;

        private readonly IReplicator _target;

        public Scheduler(IReplicator target, TimeSpan? interval = null)
        {
            _target = target;

            var intervalMs = (interval ?? DefaultInterval).TotalMilliseconds;
            _timer = new System.Timers.Timer(intervalMs);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        private void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            _target.Replicate();
        }

        public void OnExit(object? sender, ConsoleCancelEventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
        }

    }
}