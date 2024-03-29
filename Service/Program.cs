﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Extensions.HostExtensions;
using Service.Extensions.LoggerExtensions;
using Service.Models;
using Service.Services;
using Service.Services.Directum;
using Service.Services.RabbitMQ;
using Service.Services.TaskQueue;
using Service.Services.Workers;
using System.Threading.Tasks;

namespace Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(confBuilder =>
                {
                    confBuilder.AddJsonFile("config.json");
                    confBuilder.AddCommandLine(args);
                })
                .ConfigureLogging(configLogging =>
                {
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                    configLogging.AddFile();
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<TaskSchedulerService>();
                    services.AddHostedService<WorkerService>();

                    services.AddSingleton<Settings>();
                    services.AddSingleton<RMQConnectionSettings>();
                    services.AddSingleton<RMQChannelSettings>();
                    services.AddSingleton<DirectumWebServiceSettings>();
                    services.AddSingleton<ITaskProcessor, TaskProcessor>();
                    services.AddSingleton<IConnector, Connector>();
                    services.AddSingleton<RpcClient>();
                    services.AddSingleton<Consumer>();
                    services.AddSingleton<WebService>();
                    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
                });

            await builder.RunService();
        }
    }
}
