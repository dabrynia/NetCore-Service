using RabbitMQ.Client;

namespace Service.Services.RabbitMQ
{
    public interface IConnector
    {
        IConnection Connection { get; }
        bool IsConnected { get; }

        void Close();
        void CreateRabbitConnection();
        void Dispose();
    }
}