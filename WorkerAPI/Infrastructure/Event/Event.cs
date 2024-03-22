using System;

namespace WorkerAPI.Infrastructure.Event
{
    public class WorkerEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int RetryCount { get; set; }
        public string WorkerName { get; set; }
        public EventStatus Status { get; set; }
    }
}