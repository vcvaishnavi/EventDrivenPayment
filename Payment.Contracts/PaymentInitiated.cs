namespace Payment.Contracts
{  
        public record PaymentInitiated(Guid PaymentId, decimal Amount, string FromAccount, string ToAccount);
}
