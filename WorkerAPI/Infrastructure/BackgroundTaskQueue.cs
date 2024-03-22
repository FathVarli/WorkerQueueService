using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WorkerAPI.Infrastructure.Event;

namespace WorkerAPI.Infrastructure
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<WorkerEvent>> _queueHub = new ();
        
        public WorkerEvent GetWorkerEvent(string queueName)
        {
            if (!_queueHub.TryGetValue(queueName, out var queue)) return null;
            return queue.TryPeek(out var workerEvent) ? workerEvent : null;
        }

        public void QueueEvent(string queueName,WorkerEvent workerEvent)
        {
            if (workerEvent is null)
                return;
            if (_queueHub.TryGetValue(queueName, out var queue))
                queue.Enqueue(workerEvent);
            else
            {
                var newQueue = new ConcurrentQueue<WorkerEvent>();
                newQueue.Enqueue(workerEvent);
                _queueHub.TryAdd(queueName, newQueue);
            }
            
        }

        public WorkerEvent Dequeue(string queueName)
        {
            if (!_queueHub.TryGetValue(queueName, out var queue)) return null;
            if (!queue.TryDequeue(out var removedWorkerEvent)) return null;
            if (!queue.IsEmpty) return null;
            return _queueHub.TryRemove(queueName, out queue) ? removedWorkerEvent : null;
        }

        public void RemoveEvent(string queueName , Guid workerEventId)
        {
            if (!_queueHub.TryGetValue(queueName, out var queue)) return;
            var removedWorkerEvent = queue.FirstOrDefault(x => x.Id == workerEventId);
            if (removedWorkerEvent is null)
                return;
            
            var tempList = new List<WorkerEvent>();
            while (queue.TryDequeue(out var dequeuedValue))
            {
                if (dequeuedValue.Id != removedWorkerEvent.Id)
                {
                    tempList.Add(dequeuedValue);
                }
            }
            
            foreach (var item in tempList)
            {
                queue.Enqueue(item);
            }

        }

        public bool CheckTrigger(string queueName , string workerName)
        {
            if (!_queueHub.TryGetValue(queueName, out var queue)) return false;
            if (!queue.TryPeek(out var workerEvent)) return false;
            if (!workerEvent.WorkerName.Equals(workerName) || workerEvent.Status != EventStatus.Waiting) return false;
            
            var isAlreadyTriggeredSameEvent = queue.Any(x => x.WorkerName.Equals(workerName) && x.Status == EventStatus.InProgress);
            if (!isAlreadyTriggeredSameEvent) return true;
            if (!queue.TryDequeue(out var removedWorkerEvent)) return false;
            queue.Enqueue(removedWorkerEvent);
            return false;
        }
    }
}