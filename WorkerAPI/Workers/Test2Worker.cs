using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkerAPI.Infrastructure;
using WorkerAPI.Infrastructure.Event;

namespace WorkerAPI.Workers
{
    public class Test2Worker : BackgroundService
    {
        private readonly ILogger<Test2Worker> _logger;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        public Test2Worker(ILogger<Test2Worker> logger, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _logger = logger;
            _backgroundTaskQueue = backgroundTaskQueue;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var isTrigger = _backgroundTaskQueue.CheckTrigger(nameof(Test2Worker) ,nameof(Test2Worker));
                if (isTrigger)
                {
                   var workerEvent = _backgroundTaskQueue.GetWorkerEvent(nameof(Test2Worker));
                    try
                    {
                        _backgroundTaskQueue.Dequeue(nameof(Test2Worker));
                        workerEvent.Status = EventStatus.InProgress;
                        _backgroundTaskQueue.QueueEvent(nameof(Test2Worker),workerEvent);
                        _logger.LogInformation("Worker2 running at: {time} and Id: {id}", DateTimeOffset.Now, workerEvent.Id);
                        _backgroundTaskQueue.RemoveEvent(nameof(Test2Worker),workerEvent.Id);
                    }
                    catch (Exception e)
                    {
                        _backgroundTaskQueue.RemoveEvent(nameof(Test2Worker),workerEvent.Id);
                        workerEvent.Status = EventStatus.Cancelled;
                        _backgroundTaskQueue.QueueEvent(nameof(Test2Worker),workerEvent);
                    }
                }
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}