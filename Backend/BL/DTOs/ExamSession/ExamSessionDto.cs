using System.Collections.Generic;

namespace BL.DTOs.ExamSession
{
    public class ExamSessionDto
    {
        public int ExamId { get; set; }
        public long SessionId { get; set; }
        public decimal? Score { get; set; }
        public int? TotalQuestions { get; set; }
        public bool? Passed { get; set; }

        public StudySessionDto StudySession { get; set; }

        public ICollection<SessionAnswerDto> SessionAnswers { get; set; } = new List<SessionAnswerDto>();
    }
}