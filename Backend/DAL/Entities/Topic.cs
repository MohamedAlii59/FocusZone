using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class Topic
    {
        public int TopicId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; } // Domain, Concept, Technique, Tool, Career
        public int Difficulty { get; set; } = 1;
        public decimal EstimatedHours { get; set; } = 4.00m;

        // Navigation Properties
        public virtual ICollection<UserTopicMastery> UserMasteries { get; set; } = new List<UserTopicMastery>();
        public virtual ICollection<TopicRelationship> SourceRelationships { get; set; } = new List<TopicRelationship>();
        public virtual ICollection<TopicRelationship> TargetRelationships { get; set; } = new List<TopicRelationship>();
        public virtual ICollection<ResourceTopicCoverage> ResourceCoverages { get; set; } = new List<ResourceTopicCoverage>();
        public virtual ICollection<Evidence> Evidence { get; set; } = new List<Evidence>();
        public virtual ICollection<UserDomain> UserDomains { get; set; } = new List<UserDomain>();
    }
}
