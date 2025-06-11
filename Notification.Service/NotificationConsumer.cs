using MassTransit;
using Payment.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Service
{
    public class NotificationConsumer : IConsumer<PaymentProcessed>
    {
        public async Task Consume(ConsumeContext<PaymentProcessed> context)
        {
            try
            {
                var evt = context.Message;
                
                // Simulate potential notification errors
                if (evt.PaymentId == Guid.Empty)
                {
                    throw new InvalidOperationException("Invalid payment ID.");
                }

                // Simulate notification processing
                await Task.Delay(500); // Simulate some processing time

                Console.WriteLine($"[Notification] Payment {evt.PaymentId}: {evt.Message}");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"[Notification] Error processing notification: {ex.Message}");

                // Re-throw the exception to trigger the retry policy
                throw;
            }
        }
    }
}
