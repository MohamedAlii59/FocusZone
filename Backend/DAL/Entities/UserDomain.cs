using System;

namespace DAL.Entities
{
    public class UserDomain
    {
        public string UserId { get; set; } // Foreign key to AspNetUsers
        public int TopicId { get; set; } // Domain topic
        public decimal Score { get; set; } = 0.00m; // 0.00 - 1.00

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Topic Topic { get; set; }
    }
}
