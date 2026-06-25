using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class StudySession
    {
        public long SessionId { get; set; }
        public string UserId { get; set; } // Foreign key to AspNetUsers
        public long? ResourceId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string SessionSummary { get; set; }

        // Computed Property (in SQL: DATEDIFF(minute, StartedAt, EndedAt))
        public int? DurationMinutes
        {
            get
            {
                if (EndedAt.HasValue)
                {
                    return (int)(EndedAt.Value - StartedAt).TotalMinutes;
                }
                return null;
            }
        }

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Resource Resource { get; set; }
        public virtual ICollection<Evidence> Evidence { get; set; } = new List<Evidence>();
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
