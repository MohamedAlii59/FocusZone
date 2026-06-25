using System;

namespace DAL.Entities
{
    public class ResourceTopicCoverage
    {
        public long ResourceId { get; set; }
        public int TopicId { get; set; }
        public decimal CoverageWeight { get; set; } = 1.00m; // 0.00 - 1.00
        public decimal DifficultyContribution { get; set; } = 1.00m; // 0.00 - 1.00

        // Navigation Properties
        public virtual Resource Resource { get; set; }
        public virtual Topic Topic { get; set; }
    }
}
