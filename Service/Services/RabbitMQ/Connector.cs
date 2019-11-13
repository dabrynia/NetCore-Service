using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Service.Models;
using System;

namespace Service.Services.RabbitMQ
{
    public class Connector : IDisposable, IConnector
    {
        private readonly ILogger<Connector> _logger;
        private readonly RMQConnectionSettings _connectionSettings;

        public IConnection Connection { get; private set; }
        public bool IsConnected { get; private set; } = false;

        public Connector(ILogger<Connector> logger, RMQConnectionSettings connectionSettings)
        {
            this._logger = logger;
            this._connectionSettings = connectionSettings;
        }

        public void Close()
        {
            _logger.LogInformation($"Close connection to RAbbitMQ server({_connectionSettings.HostName})");
            Connection.Close();

            IsConnected = false;
        }

        public void CreateRabbitConnection()
        {
            try
            {
                ConnectionFactory factory = new ConnectionFactory
                {
                    UserName = _connectionSettings.UserName,
                    Password = _connectionSettings.Password,
                    HostName = _connectionSettings.HostName,
                };

                Connection = factory.CreateConnection();
                IsConnected = true;

                _logger.LogInformation($"Create connection to RAbbitMQ server({_connectionSettings.HostName})");
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Connection error to server({_connectionSettings.HostName}): {e.Message}");
                IsConnected = false;
                throw;
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Close();
            Connection.Dispose();
        }
        #endregion
    }
}
