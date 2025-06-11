using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Payment.Contracts;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public PaymentController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> InitiatePayment()
        {
            var paymentId = Guid.NewGuid();

            var payment = new PaymentInitiated(
                paymentId,
                100.00m,
                "Alice",
                "Bob"
            );

            await _publishEndpoint.Publish(payment);
            return Ok(new {Message = "Payment initiated successfully", PaymentId = paymentId});
        }
    }
}
