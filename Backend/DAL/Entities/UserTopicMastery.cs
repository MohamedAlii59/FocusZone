using System;

namespace DAL.Entities
{
    public class UserTopicMastery
    {
        public string UserId { get; set; } // Foreign key to AspNetUsers
        public int TopicId { get; set; }
        public decimal Mastery { get; set; } = 0.00m; // 0.00 - 1.00
        public decimal Confidence { get; set; } = 0.00m; // 0.00 - 1.00
        public decimal Interest { get; set; } = 0.50m; // 0.00 - 1.00
        public int EvidenceCount { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Topic Topic { get; set; }
    }
}
