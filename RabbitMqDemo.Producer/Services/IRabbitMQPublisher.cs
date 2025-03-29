using RabbitMqDemo.Producer.Models;

namespace RabbitMqDemo.Producer.Services
{
    public interface IRabbitMQPublisher
    {
        void Publish(OrderMessage message);
    }
}
