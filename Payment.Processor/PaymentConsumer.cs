using MassTransit;
using Payment.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Processor
{
    public class PaymentConsumer : IConsumer<PaymentInitiated>
    {
        public async Task Consume(ConsumeContext<PaymentInitiated> context)
        {
            try
            {
                var evt = context.Message;
                Console.WriteLine($"[Processor] Payment from {evt.FromAccount} to {evt.ToAccount} of {evt.Amount} received.");

                //Here you would typically call a payment gateway API to process the payment.
                // For this example, we will simulate a successful payment processing.
                await Task.Delay(1000); // Simulate some processing time

                // Simulate potential payment processing errors
                if (evt.Amount <= 0)
                {
                    throw new InvalidOperationException("Payment amount must be greater than zero.");
                }

                if (string.IsNullOrEmpty(evt.FromAccount) || string.IsNullOrEmpty(evt.ToAccount))
                {
                    throw new InvalidOperationException("Both from and to accounts must be specified.");
                }

                var processed = new PaymentProcessed(evt.PaymentId, true, "Payment successful");
                await context.Publish(processed);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"[Processor] Error processing payment: {ex.Message}");

                // Publish a failed payment event
                var failed = new PaymentProcessed(context.Message.PaymentId, false, $"Payment failed: {ex.Message}");
                await context.Publish(failed);

                // Re-throw the exception to trigger the retry policy
                throw;
            }
        }
    }
}
