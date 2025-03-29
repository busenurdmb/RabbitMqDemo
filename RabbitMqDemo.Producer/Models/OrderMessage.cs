namespace RabbitMqDemo.Producer.Models
{
    public class OrderMessage
    {
        public Guid OrderId { get; set; } = Guid.NewGuid();
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
