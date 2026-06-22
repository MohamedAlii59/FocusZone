using System;

namespace BL.DTOs
{
    public class SubscriptionDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserSubscriptionResponseDto
    {
        public int? ActiveSubscriptionId { get; set; }
        public string ActiveSubscriptionName { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public int SessionMinutes { get; set; }
        public bool IsPaidUser { get; set; }
    }
}
