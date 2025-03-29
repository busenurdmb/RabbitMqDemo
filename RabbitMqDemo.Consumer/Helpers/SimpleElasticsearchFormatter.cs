using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitMqDemo.Consumer.Helpers
{
    public class SimpleElasticsearchFormatter : ITextFormatter
    {
        public void Format(LogEvent logEvent, TextWriter output)
        {
            var log = new Dictionary<string, object?>
            {
                ["@timestamp"] = logEvent.Timestamp.UtcDateTime,      // Log zamanı (UTC)
                ["level"] = logEvent.Level.ToString(),                // Log seviyesi (Information, Warning, Error...)
                ["message"] = logEvent.RenderMessage()                // Gerçek log mesajı (formatlanmış)
            };

            // Eğer log içerisinde "Order" ile başlayan özel alanlar varsa onları da ekle
            foreach (var prop in logEvent.Properties)
            {
                if (prop.Key.StartsWith("Order"))                    // Sadece "Order" alanlarını dahil et
                    log[prop.Key.Replace("Order.", "")] = prop.Value.ToString().Trim('"');
                // Tırnakları kaldır, değeri ekle
            }

            var json = JsonSerializer.Serialize(log);                // Sade dictionary’i JSON’a çevir
            output.WriteLine(json);                                  // Elasticsearch'e gönderilecek JSON çıktısı
        }
    }

}
