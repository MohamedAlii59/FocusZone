using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class SubscriptionPlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public int SessionMinutes { get; set; }
        public PlanType PlanType { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }

    public enum PlanType
    {
        Monthly = 1,
        Yearly = 2,
        OneTime = 3
    }
}
