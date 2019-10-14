using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Models;
using Service.Services.TaskQueue;
using Service.Services.Workers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Services
{
    public class TaskSchedulerService : IHostedService, IDisposable
    {
        private Timer timer;
        private readonly IServiceProvider services;
        private readonly Settings settings;
        private readonly ILogger<TaskSchedulerService> logger;
        private readonly Random random = new Random();
        private readonly object syncRoot = new object();

        public TaskSchedulerService(IServiceProvider services)
        {
            this.services = services;
            this.settings = services.GetRequiredService<Settings>();
            this.logger = services.GetRequiredService<ILogger<TaskSchedulerService>>();
        }

        private void ProcessTask()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                logger.LogInformation("Process task started");

                DoWork();

                logger.LogInformation("Process task finished");
                Monitor.Exit(syncRoot);
            }
            else
            {
                logger.LogInformation("Processing is currently in progress. Skippeed");
            }
        }

        private void DoWork()
        {
            var number = random.Next();

            var processor = services.GetRequiredService<TaskProcessor>();
            var queue = services.GetRequiredService<IBackgroundTaskQueue>();

            queue.QueueBackgroundWorlItem(token => 
            {
                return processor.RunAsync(number, token);
            });
        }

        #region IDisposable
        public void Dispose()
        {
            timer?.Dispose();
        }
        #endregion

        #region IHostedService
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var interval = settings?.RunInterval ?? 0;
            if (interval == 0)
            {
                logger.LogWarning("Check Interval is not defined in settings. Set to default: 60 sec.");
                interval = 60;
            }

            timer = new Timer(
                e => ProcessTask(), 
                null, 
                TimeSpan.Zero, 
                TimeSpan.FromSeconds(interval));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
        #endregion
    }
}
