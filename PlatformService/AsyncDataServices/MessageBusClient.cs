using PlatformService.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PlatformService.AsyncDataServices;

public class MessageBusClient : IMessageBusClient, IDisposable
{
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;

    public MessageBusClient(IConfiguration configuration)
    {
        _configuration = configuration;
        _ = InitializeAsync(); // Fire and forget initialization
    }

    private async Task InitializeAsync()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQHost"],
            Port = int.Parse(_configuration["RabbitMQPort"])
        };

        try
        {
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);
            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;
            Console.WriteLine("--> Connected to MessageBus");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
        }
    }

    public async Task PublishNewPlatformAsync(PlatformPublishedDto platformPublishedDto)
    {
        var message = JsonSerializer.Serialize(platformPublishedDto);
        if (_connection?.IsOpen == true)
        {
            Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
            await SendMessageAsync(message);
        }
        else
        {
            Console.WriteLine("--> RabbitMQ connection is closed, not sending");
        }
    }

    // Synchronous version for backward compatibility
    public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
    {
        PublishNewPlatformAsync(platformPublishedDto).GetAwaiter().GetResult();
    }

    private async Task SendMessageAsync(string message)
    {
        if (_channel == null) return;

        var body = Encoding.UTF8.GetBytes(message);
        await _channel.BasicPublishAsync(exchange: "trigger",
                        routingKey: "",
                        body: body);
        Console.WriteLine($"--> We have sent {message}");
    }

    public void Dispose()
    {
        Console.WriteLine("MessageBus Disposed");
        try
        {
            if (_channel?.IsOpen == true)
            {
                _channel.CloseAsync().GetAwaiter().GetResult();
            }
            if (_connection?.IsOpen == true)
            {
                _connection.CloseAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Error disposing MessageBus: {ex.Message}");
        }
        finally
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }

    private Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> RabbitMQ Connection Shutdown");
        return Task.CompletedTask;
    }
}