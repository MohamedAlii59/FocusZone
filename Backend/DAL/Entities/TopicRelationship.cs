using System;

namespace DAL.Entities
{
    public class TopicRelationship
    {
        public int RelationshipId { get; set; }
        public int SourceTopicId { get; set; }
        public int TargetTopicId { get; set; }
        public string RelationshipType { get; set; } // contains, prerequisite_for, required_for, related_to
        public decimal Weight { get; set; } = 1.00m; // 0.00 - 1.00

        // Navigation Properties
        public virtual Topic SourceTopic { get; set; }
        public virtual Topic TargetTopic { get; set; }
    }
}
