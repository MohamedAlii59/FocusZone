using System;

namespace BL.DTOs
{
    public class SubscriptionPlanDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public int SessionMinutes { get; set; }
        public string PlanType { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateSubscriptionPlanDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public int SessionMinutes { get; set; }
        public int PlanType { get; set; }
    }
}
