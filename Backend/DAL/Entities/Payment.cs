using System;

namespace DAL.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public int? SubscriptionPlanId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public int? MinutesPurchased { get; set; }
        public string StripePaymentIntentId { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedOn { get; set; }
    }

    public enum PaymentType
    {
        Subscription = 1,
        MinutesPurchase = 2
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }
}
