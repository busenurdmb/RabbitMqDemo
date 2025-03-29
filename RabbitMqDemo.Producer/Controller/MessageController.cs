using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace RabbitMqDemo.Producer.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        [HttpPost("send")]
        public IActionResult SendMessage([FromBody] string message)
        {
            Log.Information("Yeni mesaj gönderildi: {Message}", message);

            // RabbitMQ’ya gönderme işlemi burada yapılabilir

            return Ok(new { status = "success", message = message });
        }
    }
}
