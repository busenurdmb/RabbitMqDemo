# 📨 .NET 9 + RabbitMQ + SQL + Elasticsearch Entegrasyonu

Bu projede RabbitMQ üzerinden iletilen sipariş mesajları SQL Server'a kaydedilir, Serilog ile hem dosyaya hem Elasticsearch'e loglanır ve Kibana'da görselleştirilir.

---

## 🌟 Amaç

.NET 8 ile geliştirilen bu mikroservis tabanlı örnek projede, RabbitMQ kullanılarak sipariş mesajları kuyruğa eklenir, SQL Server'a kaydedilir, log'lar Serilog ile Elasticsearch'e gönderilir ve Kibana üzerinden görsel analiz yapılır.

---

## 📁 Proje Klasör Yapısı

```
RabbitMqDemo/
├── RabbitMqDemo.Producer       --> API: Sipariş gönderimi
│   ├── Controllers             --> OrderController
│   ├── Services                --> RabbitMQ Publisher
│   ├── Logs                    --> Serilog File Logları
│   └── Program.cs             --> API Başlangıç noktası
│
├── RabbitMqDemo.Consumer      --> Worker: Sipariş tüketimi
│   ├── Worker.cs              --> RabbitMQ consumer
│   ├── Data/OrderDbContext.cs --> SQL bağlantısı
│   ├── Helpers                --> SimpleElasticsearchFormatter.cs
│   ├── Logs                   --> Elasticsearch loglama
│   └── Program.cs
│
├── docker-compose.yml         --> Elasticsearch, Kibana, RabbitMQ servisleri
├── setup.bat                  --> Visual Studio hızlı başlatma
└── RabbitMqDemo.sln           --> Solution dosyası
```

---

## 💠 Proje Teknolojileri

| Teknoloji         | Açıklama                                     |
|------------------|----------------------------------------------|
| 💻 .NET 9         | Web API ve Worker Service                    |
| 🐇 RabbitMQ       | Mesaj kuyruğu sistemi                        |
| 🗃️ SQL Server     | Sipariş verilerinin kaydedildiği veritabanı  |
| 🧾 Serilog        | Gelişmiş loglama altyapısı                   |
| 🔍 Elasticsearch  | Log verilerinin indekslendiği arama motoru   |
| 📊 Kibana         | Elasticsearch verilerinin görselleştirilmesi |
| 🐳 Docker         | Container ortamında servis çalıştırma        |

---
![Kibana index](https://github.com/busenurdmb/RabbitMqDemo/blob/master/image/api2.jpeg)
![Kibana index](https://github.com/busenurdmb/RabbitMqDemo/blob/master/image/AP%C4%B01.png)
![Kibana dasboard](https://github.com/busenurdmb/RabbitMqDemo/blob/master/image/rabbitmq.png)
![Kibana dasboard](https://github.com/busenurdmb/RabbitMqDemo/blob/master/image/rabbitmq2.png)

## ⚙️ Süreç Akışı

1. 🍭 Kullanıcı bir sipariş gönderir (POST /api/order)
2. 📬 Sipariş, RabbitMQ kuyruğuna eklenir
3. 🎯 Worker Service, kuyruğu dinler ve mesajı alır
4. 🗃 SQL Server'a veri kaydı yapılır
5. 🧾 Serilog, log'u hem dosyaya hem Elasticsearch'e yollar
6. 📊 Kibana üzerinden loglar analiz edilir

---

## 🧾 Loglama

### Serilog Konfigürasyonu:
```csharp
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
```

### 🧹 SimpleElasticsearchFormatter.cs

```json
{
  "@timestamp": "2025-03-29T18:34:48.975Z",
  "level": "Information",
  "message": "📥 Sipariş işlendi: Kalem",
  "Order.ProductName": "Kalem",
  "Order.Quantity": 30,
  "Order.Price": 600
}
```
![Kibana dasboard](https://github.com/busenurdmb/RabbitMqDemo/blob/master/image/loglama.png)


## 📊 Kibana Dashboard

📦 Ürün bazlı sipariş dağılımı (Pie chart)  
💰 Fiyat bazlı dağılım (Bar chart)  
⏱ Zaman bazlı sipariş analizi (Time series)

![Kibana dasboard](https://github.com/busenurdmb/RabbitMqDemo/blob/master/image/kibanadahboard.png)
![Kibana index](https://github.com/busenurdmb/RabbitMqDemo/blob/master/image/kibanaindex.png)

---

## 🐳 Docker Compose

```yaml
version: "3.7"
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:7.17.0
    container_name: elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - ES_JAVA_OPTS=-Xms512m -Xmx512m
    ports:
      - "9200:9200"

  kibana:
    image: docker.elastic.co/kibana/kibana:7.17.0
    container_name: kibana
    ports:
      - "5601:5601"
    depends_on:
      - elasticsearch
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200

  rabbitmq:
    image: rabbitmq:3-management
    container_name: rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
```

```bash
docker-compose up -d
```

- http://localhost:9200 → Elasticsearch
- http://localhost:5601 → Kibana
- http://localhost:15672 → RabbitMQ

---
![Kibana index](https://github.com/busenurdmb/RabbitMqDemo/blob/master/image/r1.png)
![Kibana index](https://github.com/busenurdmb/RabbitMqDemo/blob/master/image/rabbitmqdocker.png)

## 📬 Otomatik Sipariş Gönderimi (Test Amaçlı)

```csharp
app.Lifetime.ApplicationStarted.Register(() =>
{
    var publisher = app.Services.GetRequiredService<IRabbitMQPublisher>();

    for (int i = 0; i < 50; i++)
    {
        publisher.Publish(new OrderMessage {
            OrderId = Guid.NewGuid(),
            ProductName = RandomProduct(),
            Quantity = Random.Shared.Next(1, 50),
            Price = Random.Shared.Next(100, 2000),
            CreatedAt = DateTime.UtcNow
        });
    }
});
```

---

## 📥 Örnek Sipariş JSON

```json
{
  "productName": "Kalem",
  "quantity": 20,
  "price": 200
}
```

---



---

## ⭐ Katkı Sağlamak

Bu projeye katkı sağlamak isterseniz PR gönderebilir veya issue oluşturabilirsiniz.

Hazırlayan: **@busenurdmb**  
Lisans: MIT



