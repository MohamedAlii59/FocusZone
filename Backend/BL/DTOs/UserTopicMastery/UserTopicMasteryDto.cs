using System;

namespace BL.DTOs.UserTopicMastery
{
    public class UserTopicMasteryDto
    {
        public string UserId { get; set; }
        public int TopicId { get; set; }
        public decimal Mastery { get; set; }
        public decimal Confidence { get; set; }
        public decimal Interest { get; set; }
        public int EvidenceCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}