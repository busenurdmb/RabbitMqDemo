using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMqDemo.Producer.Models;
using RabbitMqDemo.Producer.Services;

namespace RabbitMqDemo.Producer.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IRabbitMQPublisher _publisher;
        private readonly ILogger<OrderController> _logger;
        public OrderController(IRabbitMQPublisher publisher, ILogger<OrderController> logger)
        {
            _publisher = publisher;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] OrderMessage order)
        {
            try
            {
                _logger.LogInformation("📤 Yeni sipariş gönderiliyor: {@Order}", order);
                _publisher.Publish(order);
                _logger.LogInformation("✅ Sipariş kuyruğa gönderildi: {OrderId}", order.OrderId);

                return Ok(new { message = "Order published to RabbitMQ", orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Sipariş gönderilirken hata oluştu.");
                return StatusCode(500, "Bir hata oluştu.");
            }
            
        }
    }
}
