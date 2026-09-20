//RabbitMq Publisher Service
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public class RabbitMqPublisher
{
    private readonly ConnectionFactory _factory;
    public RabbitMqPublisher(IConfiguration configuration)
    {
        var host = configuration["RABBITMQ_HOST"]
            ?? throw new InvalidOperationException("RABBITMQ_HOST is not configured.");

        var username = configuration["RABBITMQ_USERNAME"]
            ?? throw new InvalidOperationException("RABBITMQ_USERNAME is not configured.");

        var password = configuration["RABBITMQ_PASSWORD"]
            ?? throw new InvalidOperationException("RABBITMQ_PASSWORD is not configured.");

        _factory = new ConnectionFactory
        {
            HostName = host,
            Port = 5672,
            UserName = username,
            Password = password
        };
    }
        public async Task PublishAsync(ClickEvent clickEvent)
    {
        using var connection = await _factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: "MessageBroker",
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(clickEvent));
        await channel.BasicPublishAsync(exchange: "",
                                    routingKey: "MessageBroker",
                                    body: body);                               
    }
    }
