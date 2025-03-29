using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Security.Cryptography;
using Serilog;
using RabbitMqDemo.Consumer.Data;
using RabbitMqDemo.Consumer.Models;
using System.Text.Json;

namespace RabbitMqDemo.Consumer;
//BackgroundService: .NET 8’de sürekli çalışan, arka planda iş yapan servislerin temel sınıfı
//RabbitMQ mesajlarını dinlemek için ideal yapı.
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IConnection _connection;
    private IModel _channel;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var factory = new ConnectionFactory()
        {
            HostName = "localhost", // docker'daysa: localhost
            Port = 5673,            // docker kullanıyorsan bu!
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();


        //Eğer bu kuyruk Producer tarafından oluşturulmadıysa, burada oluşturulsun.
        // Varsa zaten bir şey yapmaz.İdempotent'tir.
        _channel.QueueDeclare(
            queue: "order_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
        _serviceProvider = serviceProvider;
    }

    //ExecuteAsync – Mesaj Dinleme  ->BackgroundService başladığında bu metot tetiklenir. ->Kuyruğu dinlemeye başlar.
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //RabbitMQ’nun event tabanlı tüketici yapısıdır.
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            //ea (event args) içinde gelen mesaj bilgileri var.
            //body ? byte[] formatındadır. UTF8.GetString(...) ile okunabilir.
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            try
            {
                var orderMessage = JsonSerializer.Deserialize<Order>(json);
                orderMessage.CreatedAt = DateTime.Now;

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                db.Orders.Add(orderMessage);
                await db.SaveChangesAsync();

                //_logger.LogInformation("? Sipariş SQL'e kaydedildi: {@Order}", orderMessage);

                Log.ForContext("Order.ProductName", orderMessage.ProductName, true)
     .ForContext("Order.Quantity", orderMessage.Quantity, true)
     .ForContext("Order.Price", orderMessage.Price, true)
     .Information("📦 Sipariş işlendi: {ProductName}", orderMessage.ProductName);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Sipariş SQL'e kaydedilirken hata oluştu");
            }


            //_logger.LogInformation("?? Mesaj alındı: {Message}", json);
        };

        _channel.BasicConsume(queue: "order_queue", autoAck: true, consumer: consumer);
        //queue ->Dinlenecek kuyruk adı
        //autoAck: true ->Mesaj alındığında otomatik "okundu" sayılır
        //consumer ->	Hangi consumer ile dinlenecek

        return Task.CompletedTask;
    }
   
    //Uygulama kapanırken kanal ve bağlantılar düzgünce kapatılır.
    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

