using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Service.Models;
using Service.Services.Directum;
using Service.Services.TaskQueue;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Services.RabbitMQ
{
    public class Consumer : IDisposable
    {
        private readonly ILogger<Consumer> _logger;
        private readonly RMQChannelSettings _channelSettings;
        private readonly IConnector _connector;
        private readonly IServiceProvider _service;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        private IModel _channel;
        private EventingBasicConsumer _consumer;

        public IConnection Connection { get { return _connector.Connection; } }

        public Consumer(IServiceProvider services)
        {
            _service = services;
            _logger = services.GetRequiredService<ILogger<Consumer>>();
            _connector = services.GetRequiredService<IConnector>();
            _channelSettings = services.GetRequiredService<RMQChannelSettings>();
        }

        public async Task CallAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            CreateChannel();
            CreateConsumer();

            await Task.Run(() =>
            {
                // не позволяет подписчу отдавать больше одного сообщения единовременно
                // т.е. пока не обработает предыдущение не получит новое из очереди
                // по умолчанию, все сообщения автоматически распределяются по всем получателям
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                // autoAck - флаг отвечающий за автоматическое подтверждение сообщений
                // false - подтверждение о получении только после выполнения задачи
                // Подтверждение должно быть отправлено по тому же каналу, который получил доставку. 
                // Попытки подтвердить использование другого канала приведут к исключению протокола уровня канала. 
                _channel.BasicConsume(queue: _channelSettings.QueueName,
                                        autoAck: false,
                                        consumer: _consumer);
            }, token);
        }

        public void Close()
        {
            _channel.Close();
        }

        #region private methods
        private void CreateConsumer()
        {
            var queue = _service.GetRequiredService<IBackgroundTaskQueue>();
            var webService = _service.GetRequiredService<WebService>();

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);

                queue.QueueBackgroundWorlItem(token =>
                {
                    return webService.RunAsync(response, token);
                });

                try
                {
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            };
        }

        private void CreateChannel()
        {
            CreateChannel(exchange: _channelSettings.ExchangeName, type: _channelSettings.Type, durable: _channelSettings.Durable);
        }

        private void CreateChannel(string exchange, string type, bool durable)
        {
            _channel = Connection.CreateModel();

            _channel.ExchangeDeclare(exchange: exchange,
                                     type: type,
                                     durable: durable);
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
