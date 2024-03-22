using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkerAPI.Infrastructure;
using WorkerAPI.Infrastructure.Event;

namespace WorkerAPI.Workers
{
    public class TestWorker : BackgroundService
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly ILogger<TestWorker> _logger;

        public TestWorker(ILogger<TestWorker> logger, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _logger = logger;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var isTrigger = _backgroundTaskQueue.CheckTrigger(nameof(TestWorker), nameof(TestWorker));
                if (isTrigger)
                {
                    var workerEvent = _backgroundTaskQueue.GetWorkerEvent(nameof(TestWorker));
                    try
                    {
                        _backgroundTaskQueue.Dequeue(nameof(TestWorker));
                        workerEvent.Status = EventStatus.InProgress;
                        _backgroundTaskQueue.QueueEvent(nameof(TestWorker), workerEvent);
                        _logger.LogInformation("Worker running at: {time} and Id: {id}", DateTimeOffset.Now,
                            workerEvent.Id);
                        _backgroundTaskQueue.RemoveEvent(nameof(TestWorker), workerEvent.Id);
                    }
                    catch (Exception e)
                    {
                        _backgroundTaskQueue.RemoveEvent(nameof(TestWorker), workerEvent.Id);
                        workerEvent.Status = EventStatus.Cancelled;
                        _backgroundTaskQueue.QueueEvent(nameof(TestWorker), workerEvent);
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}