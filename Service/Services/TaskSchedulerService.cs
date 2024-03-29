﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Models;
using Service.Services.RabbitMQ;
using Service.Services.TaskQueue;
using Service.Services.Workers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Services
{
    /// <summary>
    /// Помещает задачи в очередь
    /// </summary>
    public class TaskSchedulerService : IHostedService, IDisposable
    {
        private Timer timer;
        private readonly IServiceProvider services;
        private readonly Settings settings;
        private readonly ILogger<TaskSchedulerService> logger;
        private readonly IConnector connector;
        private readonly Random random = new Random();
        private readonly object syncRoot = new object();

        public TaskSchedulerService(IServiceProvider services)
        {
            this.services = services;
            this.settings = services.GetRequiredService<Settings>();
            this.logger = services.GetRequiredService<ILogger<TaskSchedulerService>>();
            this.connector = services.GetRequiredService<IConnector>();
        }

        private void ProcessTask()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                logger.LogInformation("Process task started");

                if (!connector.IsConnected) connector.CreateRabbitConnection();

                CheckRabbitMQ();

                //for (int i = 0; i < 3; ++i) DoWork();

                logger.LogInformation("Process task finished");
                Monitor.Exit(syncRoot);
            }
            else
            {
                logger.LogInformation("Processing is currently in progress. Skippeed");
            }
        }

        private void CheckRabbitMQ()
        {
            var Consumer = services.GetRequiredService<Consumer>();
            var queue = services.GetRequiredService<IBackgroundTaskQueue>();

            queue.QueueBackgroundWorlItem(token =>
            {
                return Consumer.CallAsync(token);
            });
        }

        private void DoWork()
        {
            var number = random.Next(20);

            //var processor = services.GetRequiredService<ITaskProcessor>();
            var rpcClient = services.GetRequiredService<RpcClient>();
            var queue = services.GetRequiredService<IBackgroundTaskQueue>();

            queue.QueueBackgroundWorlItem(token =>
            {
                return rpcClient.RunAsync(number.ToString(), token);
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
