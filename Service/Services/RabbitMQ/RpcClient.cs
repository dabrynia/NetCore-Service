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
        private readonly ILogger<RpcClient> _logger;
        private readonly RMQChannelSettings _channelSettings;
        private readonly IConnector _connector;
        private readonly IModel _channel;
        private readonly string _replyQueueName;

        private EventingBasicConsumer _consumer;

        public IConnection Connection { get { return _connector.Connection; } }

        public RpcClient(ILogger<RpcClient> logger, IConnector connector, RMQChannelSettings channelSettings)
        {
            _logger = logger;
            _connector = connector;
            _channelSettings = channelSettings;

            _channel = Connection.CreateModel();
            // create a non-durable, exclusive, autodelete queue with a generated name
            _replyQueueName = _channel.QueueDeclare().QueueName;

            CreateConsumer();
        }

        public async Task RunAsync(string massege, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var messageBytes = Encoding.UTF8.GetBytes(massege);
            var correlationId = Guid.NewGuid().ToString();

            IBasicProperties properties = _channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;
            properties.ReplyTo = _replyQueueName;

            await Task.Run(() =>
            {
                _channel.BasicPublish(exchange: _channelSettings.ExchangeName,
                                      routingKey: _channelSettings.QueueName,
                                      basicProperties: properties,
                                      body: messageBytes);

                _logger.LogInformation($"[->] Sent '{_channelSettings.QueueName}':'{massege}'");

                _channel.BasicConsume(consumer: _consumer,
                                      queue: _replyQueueName,
                                      autoAck: true);

            }, token);
        }

        #region private methods
        private void CreateConsumer()
        {
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);

                _logger.LogInformation($"[<-] Response : {response}");
            };
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            _channel.Dispose();
            Connection.Dispose();
        }
        #endregion
    }
}
