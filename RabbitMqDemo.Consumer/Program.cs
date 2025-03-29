using Microsoft.EntityFrameworkCore;
using RabbitMqDemo.Consumer;
using RabbitMqDemo.Consumer.Data;
using RabbitMqDemo.Consumer.Helpers;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
 .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        AutoRegisterTemplate = false, // sade JSON gönderiyoruz
        IndexFormat = "order-logs-{0:yyyy.MM.dd}",
        CustomFormatter = new SimpleElasticsearchFormatter()
    })
    .CreateLogger();

try
{
    Log.Information("Worker baþlatýlýyor...");

    IHost host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
       .ConfigureServices((context, services) =>
       {
           services.AddDbContext<OrderDbContext>(options =>
               options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

           services.AddHostedService<Worker>();
       })
        .Build();

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker baþlatýlamadý!");
}
finally
{
    Log.CloseAndFlush();
}
