
using RabbitMqDemo.Producer.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);


// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(); // Serilog'u host'a bağla

// Add services to the container.

builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();


// ?? Swagger servislerini ekle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();


var app = builder.Build();

// ?? Swagger arayüzünü aktif et
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger JSON
    app.UseSwaggerUI(); // Swagger UI
}

app.UseHttpsRedirection();
app.UseAuthorization(); // ? BU SATIR YOK
app.MapControllers(); // <-- Bu sat?r çok önemli!

// ? Test amaçlı otomatik mesaj gönderimi
//app.Lifetime.ApplicationStarted.Register(() =>
//{
//    var publisher = app.Services.GetRequiredService<IRabbitMQPublisher>();

//    publisher.Publish(new RabbitMqDemo.Producer.Models.OrderMessage
//    {
//        OrderId = Guid.NewGuid(),
//        ProductName = "Test Ürünü",
//        Quantity = 3,
//        CreatedAt = DateTime.UtcNow
//    });

//    Console.WriteLine("? Bekleniyor...");
//    Thread.Sleep(10000); // 10 saniye bekle ki UI'da kuyruk görünür olsun
//});
//app.Lifetime.ApplicationStarted.Register(() =>
//{
//    var publisher = app.Services.GetRequiredService<IRabbitMQPublisher>();

//    var random = new Random();
//    var productNames = new[] { "Kalem", "Defter", "Silgi", "Kitap", "Çanta" };

//    for (int i = 0; i < 50; i++)
//    {
//        var order = new RabbitMqDemo.Producer.Models.OrderMessage
//        {
//            OrderId = Guid.NewGuid(),
//            ProductName = productNames[random.Next(productNames.Length)],
//            Quantity = random.Next(1, 100), // 1 ile 99 arasında
//            Price = random.Next(10, 1000),  // 10 ile 999 arasında
//            CreatedAt = DateTime.UtcNow
//        };

//        publisher.Publish(order);
//        Console.WriteLine($"📤 {i + 1}. sipariş gönderildi: {order.ProductName}, {order.Quantity} adet, {order.Price} TL");

//        Thread.Sleep(100); // çok hızlı gönderim olmasın
//    }

//    Console.WriteLine("✅ Tüm test siparişleri gönderildi.");
//});
app.Run();

