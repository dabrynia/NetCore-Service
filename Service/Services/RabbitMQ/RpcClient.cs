using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Service.Models;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Services.RabbitMQ
{
    public class RpcClient : IDisposable
    {
        private readonly ILogger<RpcClient> logger;
        private readonly RMQConnectionSettings connectionSettings;
        private readonly RMQChannelSettings channelSettings;

        private readonly IConnection connection;
        private readonly IModel channel;
        private EventingBasicConsumer consumer;

        private readonly string replyQueueName;

        public RpcClient(ILogger<RpcClient> logger, RMQConnectionSettings connectionSettings, RMQChannelSettings channelSettings)
        {
            this.logger = logger;
            this.connectionSettings = connectionSettings;
            this.channelSettings = channelSettings;

            connection = CreateRabbitConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;

            CreateConsumer();
        }

        public async Task RunAsync(string massege, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var messageBytes = Encoding.UTF8.GetBytes(massege);
            var correlationId = Guid.NewGuid().ToString();

            IBasicProperties properties = channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;
            properties.ReplyTo = replyQueueName;

            await Task.Run(() =>
            {
                channel.BasicPublish(exchange: channelSettings.ExchangeName,
                                        routingKey: channelSettings.QueueName,
                                        basicProperties: properties,
                                        body: messageBytes);

                logger.LogInformation($"[->] Sent '{channelSettings.QueueName}':'{massege}'");

                channel.BasicConsume(consumer: consumer,
                                        queue: replyQueueName,
                                        autoAck: true);

            }, token);
        }

        public void Close()
        {
            connection.Close();
        }

        #region private methods
        private IConnection CreateRabbitConnection()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                UserName = connectionSettings.UserName,
                Password = connectionSettings.Password,
                HostName = connectionSettings.HostName,
            };

            return factory.CreateConnection();
        }

        private IModel CreateRabbitChannel(IConnection connection)
        {
            IModel model = connection.CreateModel();

            return model;
        }

        private void CreateConsumer()
        {
            this.consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);

                logger.LogInformation($"[<-] Response : {response}");
            };
        }

        #endregion

        #region IDisposable
        public void Dispose()
        {
            Close();
            connection.Dispose();
        }
        #endregion
    }
}
