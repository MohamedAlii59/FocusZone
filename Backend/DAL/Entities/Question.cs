using System;

namespace DAL.Entities
{
    public class Question
    {
        public long QuestionId { get; set; }
        public string UserId { get; set; }
        public long SessionId { get; set; }
        public string QuestionText { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual StudySession StudySession { get; set; }
    }
}





