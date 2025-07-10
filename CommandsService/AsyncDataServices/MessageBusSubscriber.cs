using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CommandsService.AsyncDataServices;

public class MessageBusSubscriber : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IEventProcessor _eventProcessor;
    private IConnection? _connection;
    private IChannel? _channel;
    private string? _queueName;

    public MessageBusSubscriber(
        IConfiguration configuration,
        IEventProcessor eventProcessor)
    {
        _configuration = configuration;
        _eventProcessor = eventProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeRabbitMQAsync();

        stoppingToken.ThrowIfCancellationRequested();

        if (_channel == null || _queueName == null)
        {
            throw new InvalidOperationException("RabbitMQ not properly initialized");
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            Console.WriteLine("--> Event Received!");
            var body = ea.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());
            _eventProcessor.ProcessEvent(notificationMessage);
            await Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);

        // Keep the service running
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task InitializeRabbitMQAsync()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQHost"],
            Port = int.Parse(_configuration["RabbitMQPort"] ?? "5672")
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);
        var queueResult = await _channel.QueueDeclareAsync();
        _queueName = queueResult.QueueName;

        await _channel.QueueBindAsync(queue: _queueName,
            exchange: "trigger",
            routingKey: "");

        Console.WriteLine("--> Listening on the Message Bus...");
        _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;
    }

    private async Task RabbitMQ_ConnectionShutdownAsync(object? sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> Connection Shutdown");
        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        try
        {
            if (_channel != null)
            {
                _channel.CloseAsync().GetAwaiter().GetResult();
                _channel.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }

            if (_connection != null)
            {
                _connection.CloseAsync().GetAwaiter().GetResult();
                _connection.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during disposal: {ex.Message}");
        }

        base.Dispose();
    }
}