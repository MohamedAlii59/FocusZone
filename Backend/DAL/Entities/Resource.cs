using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class Resource
    {
        public long ResourceId { get; set; }
        public string Title { get; set; }
        public string Type { get; set; } // Youtube, Course, Book, Article, PDF, Documentation
        public string Url { get; set; }
        public int Difficulty { get; set; } = 1;
        public int Depth { get; set; } = 1;
        public int EstimatedMinutes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
        public virtual ICollection<ResourceTopicCoverage> TopicCoverages { get; set; } = new List<ResourceTopicCoverage>();
    }
}
