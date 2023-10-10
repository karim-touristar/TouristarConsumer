using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TouristarConsumer.Models;

namespace TouristarConsumer.Services;

public class ConsumerService : BackgroundService
{
    private readonly RabbitConfig _rabbitConfig;
    private ConnectionFactory _connectionFactory = null!;
    private IConnection _connection = null!;
    private IModel _channel = null!;
    private readonly string _queueName;
    private readonly ILogger _logger;

    protected ConsumerService(string queueName, RabbitConfig rabbitConfig, ILogger logger)
    {
        _queueName = queueName;
        _rabbitConfig = rabbitConfig;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = _rabbitConfig.HostName,
            Port = int.Parse(_rabbitConfig.Port),
            UserName = _rabbitConfig.Username,
            Password = _rabbitConfig.Password,
            DispatchConsumersAsync = true
        };
        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        _channel.BasicQos(0, 1, false);

        _logger.LogInformation($"Successfully initialised connection to queue {_queueName}.");

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (bc, ea) =>
        {
            _logger.LogInformation($"Received message for queue {_queueName}.");
            try
            {
                var text = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation(
                    $"About to process message with tag {ea.DeliveryTag} on queue {_queueName}.");
                await ProcessMessage(text);
                _channel.BasicAck(ea.DeliveryTag, true);
                _logger.LogInformation(
                    $"Message with tag {ea.DeliveryTag} on queue {_queueName} processed successfully.");
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    $"There was an issue processing message with tag {ea.DeliveryTag} on queue {_queueName}. Exception: {exception}.");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

        await Task.CompletedTask;
    }

    protected virtual Task ProcessMessage(string message)
    {
        throw new NotImplementedException();
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _connection.Close();
        return Task.CompletedTask;
    }
}