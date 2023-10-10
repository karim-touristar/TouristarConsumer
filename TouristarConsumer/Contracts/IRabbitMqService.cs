using RabbitMQ.Client;

namespace TouristarConsumer.Contracts;

public interface IRabbitMqService
{
    IConnection CreateChannel();
}