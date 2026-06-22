using System;

namespace DAL.Entities
{
    public class Subscription
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int SubscriptionPlanId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SubscriptionStatus Status { get; set; }
        public string StripeSubscriptionId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    public enum SubscriptionStatus
    {
        Active = 1,
        Cancelled = 2,
        Expired = 3,
        PendingPayment = 4
    }
}
