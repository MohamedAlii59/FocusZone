using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class ExamSession
    {
        public int ExamId { get; set; }
        public long SessionId { get; set; } // FK to StudySession.SessionId
        public decimal? Score { get; set; }
        public int? TotalQuestions { get; set; }
        public bool? Passed { get; set; }

        public virtual StudySession StudySession { get; set; }
        public virtual ICollection<SessionAnswer> SessionAnswers { get; set; } = new List<SessionAnswer>();
    }
}
