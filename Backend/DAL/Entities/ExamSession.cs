using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class ExamSession
    {
        public int SessionId { get; set; }
        public decimal? Score { get; set; }
        public int? TotalQuestions { get; set; }
        public bool? Passed { get; set; }

        public virtual ICollection<SessionAnswer> SessionAnswers { get; set; } = new List<SessionAnswer>();
    }
}
