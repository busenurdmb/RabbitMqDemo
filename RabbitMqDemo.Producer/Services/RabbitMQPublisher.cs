
using System.Text.Json;
using System.Text;
using RabbitMQ.Client;
using RabbitMqDemo.Producer.Models;

namespace RabbitMqDemo.Producer.Services
{
    public class RabbitMQPublisher: IRabbitMQPublisher
    {
        private readonly IConfiguration _configuration;
        private readonly IModel _channel;
        //IModel:RabbitMQ ile kanal açmak için kullanılır. Her bağlantı üzerinden kanallar oluşturulur ve işlemler bu kanallar üzerinden yapılır

        public RabbitMQPublisher(IConfiguration configuration)
        {
            _configuration = configuration;

      

            //🧩 ConnectionFactory, RabbitMQ ile bağlantı kurmak için hazır bir yapıdır.
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:Host"],
                Port = int.Parse(_configuration["RabbitMQ:Port"]),
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"]
                
            };

            try
            {
                //CreateConnection()=	RabbitMQ sunucusuna TCP bağlantısı kurar
                var connection = factory.CreateConnection();
                Console.WriteLine("✅ RabbitMQ bağlantısı kuruldu!");

                //CreateModel()=O bağlantı üzerinden bir kanal (channel) oluşturur
                _channel = connection.CreateModel();

                //QueueDeclare(...)=Eğer "order_queue" kuyruğu yoksa oluşturur. Varsa aynen devam eder

                _channel.QueueDeclare(queue: "order_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                //durable: true->RabbitMQ yeniden başlasa bile kuyruk silinmez
                //exclusive: false->Birden fazla bağlantı bu kuyruğa erişebilir
                //autoDelete: false->Kuyruğu kimse dinlemiyorsa silinmez
                //arguments: null->Opsiyonel ayarlar (TTL, DLQ vs.)
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ RabbitMQ bağlantı hatası: " + ex.Message);
            }
        }

        public void Publish(OrderMessage message)
        {

            Console.WriteLine("➡ Mesaj gönderiliyor: " + JsonSerializer.Serialize(message));

            //   Serialize ->OrderMessage objesini JSON’a çeviriyoruz
            //Encoding.UTF8.GetBytes(...)->RabbitMQ byte array formatında veri ister, o yüzden çeviriyoruz
            
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));


            //BasicPublish(...)->Mesajı doğrudan order_queue kuyruğuna gönderiyoruz
            _channel.BasicPublish(exchange: "",
                                  routingKey: "order_queue",
                                  basicProperties: null,
                                  body: body);


            //📌 exchange: "" nedir? Exchange, RabbitMQ'da gelen mesajların hangi kuyruk(lar)a yönlendirileceğini belirleyen mekanizmadır.
            //🟡 Producer mesajı doğrudan kuyruğa göndermez!
            //🟢 Producer → Exchange → Routing yapılır → Kuyruklara gider.
            //Boş exchange "default exchange" anlamına gelir.
            //Bu durumda routingKey ile doğrudan kuyruk ismini belirtiriz.
            //Yani: "Gelen mesaj hangi kuyruğa, hangi kurala göre gidecek?"
            //"exchange: "" + routingKey: "order_queue" → order_queue kuyruğuna gider.

            Console.WriteLine("✅ Mesaj RabbitMQ'ya gönderildi.");
        }
    }
}
