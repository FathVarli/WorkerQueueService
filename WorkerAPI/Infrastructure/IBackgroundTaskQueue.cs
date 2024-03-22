using System;
using WorkerAPI.Infrastructure.Event;

namespace WorkerAPI.Infrastructure
{
    public interface IBackgroundTaskQueue
    {
        WorkerEvent GetWorkerEvent(string queueName);
        void QueueEvent(string queueName, WorkerEvent workerEvent);  
        WorkerEvent Dequeue(string queueName);
        void RemoveEvent(string queueName, Guid workerEventId);
        bool CheckTrigger(string queueName , string workerName);
    }
}