using System;

namespace BL.DTOs
{
    public class PaymentIntentDto
    {
        public int PlanId { get; set; }
        public int? Minutes { get; set; }
        public string PaymentType { get; set; } // "Subscription" or "MinutesPurchase"
    }

    public class PaymentDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
    }

    public class StripeWebhookDto
    {
        public string Type { get; set; }
        public object Data { get; set; }
    }

    public class CreatePaymentIntentResponseDto
    {
        /// <summary>
        /// Stripe's hosted checkout URL. Frontend should redirect the browser to this URL.
        /// </summary>
        public string CheckoutUrl { get; set; }
        
        /// <summary>
        /// Stripe publishable key (for frontend integration if needed)
        /// </summary>
        public string PublishableKey { get; set; }
        
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }
}
