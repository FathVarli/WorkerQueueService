using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkerAPI.Infrastructure;
using WorkerAPI.Infrastructure.Event;
using WorkerAPI.Workers;

namespace WorkerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {

        private readonly ILogger<TestController> _logger;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        public TestController(ILogger<TestController> logger, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _logger = logger;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        [HttpGet]
        [Route("/test")]
        public IActionResult Get()
        {
            _backgroundTaskQueue.QueueEvent(nameof(TestWorker),new WorkerEvent
            {
                WorkerName = nameof(TestWorker),
                Status = EventStatus.Waiting,
                RetryCount = 0
            });

            return Ok("Added queue");

        }
        
        [HttpGet]
        [Route("/test2")]
        public IActionResult Get2()
        {
            _backgroundTaskQueue.QueueEvent(nameof(Test2Worker) , new WorkerEvent
            {
                WorkerName = nameof(Test2Worker),
                Status = EventStatus.Waiting,
                RetryCount = 0
            });

            return Ok("Added queue");

        }
    }
}