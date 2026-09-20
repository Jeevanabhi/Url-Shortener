using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class RabbitMqConsumer : BackgroundService
{

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    public RabbitMqConsumer(IServiceScopeFactory scopeFactory,IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = _configuration["RABBITMQ_HOST"]
                   ?? throw new InvalidOperationException("RABBITMQ_HOST is not configured.");

        var username = _configuration["RABBITMQ_USERNAME"]
                       ?? throw new InvalidOperationException("RABBITMQ_USERNAME is not configured.");

        var password = _configuration["RABBITMQ_PASSWORD"]
                       ?? throw new InvalidOperationException("RABBITMQ_PASSWORD is not configured.");

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = 5672,
            UserName = username,
            Password = password
        };
        using var connection = await factory.CreateConnectionAsync(cancellationToken: stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "MessageBroker",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken
        );
        Console.WriteLine("RabbitMQ consumer started. Waiting for messages...");
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            byte[] msgByte = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(msgByte);
            try
            {
                var clickobject = JsonSerializer.Deserialize<ClickEvent>(message);
            using var scope = _scopeFactory.CreateScope();
            var clickService = scope.ServiceProvider.GetRequiredService<ClickService>();
            await clickService.SaveClickToDb(clickobject);
            Console.WriteLine($"Message Received :{message}");

            await channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag,
                                        multiple: false,
                                        cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            

        };
        await channel.BasicConsumeAsync(queue: "MessageBroker",
                                        autoAck: false,
                                        consumer: consumer,
                                        cancellationToken: stoppingToken);
        await Task.Delay(Timeout.Infinite, cancellationToken: stoppingToken);                                

    }


}