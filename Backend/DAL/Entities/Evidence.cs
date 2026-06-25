using System;

namespace DAL.Entities
{
    public class Evidence
    {
        public long EvidenceId { get; set; }
        public long SessionId { get; set; }
        public int TopicId { get; set; }
        public string Type { get; set; } // quiz, study_time, assessment, retention_test
        public decimal Score { get; set; } // 0.00 - 1.00
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual StudySession StudySession { get; set; }
        public virtual Topic Topic { get; set; }
    }
}
