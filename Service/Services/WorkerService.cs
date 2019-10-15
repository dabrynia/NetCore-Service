using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Models;
using Service.Services.TaskQueue;

namespace Service.Services
{
    // BackgroundService - использовать для длительно выполняемых операций
    public class WorkerService : BackgroundService
    {
        private readonly IBackgroundTaskQueue taskQueue;
        private readonly ILogger<WorkerService> logger;
        private readonly Settings settings;

        public WorkerService(IBackgroundTaskQueue taskQueue, ILogger<WorkerService> logger, Settings settings)
        {
            this.taskQueue = taskQueue;
            this.logger = logger;
            this.settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var workerCount = settings.WorkersCount;
            var workers = Enumerable.Range(0, workerCount).Select(num => RunInstance(num, token));

            await Task.WhenAll(workers);
        }

        private async Task RunInstance(int num, CancellationToken token)
        {
            logger.LogInformation($"#{num} is starting");

            while(!token.IsCancellationRequested)
            {
                // извлеч из очереди
                var workItem = await taskQueue.DequeueAsync(token);

                try
                {
                    logger.LogInformation($"#{num}: Processing task. Queue size {taskQueue.Size}");
                    await workItem(token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"#{num}: Error occured executing task");
                }
            }

            logger.LogInformation($"#{num} is stopping");
        }
    }
}
