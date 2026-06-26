using System;
using System.Collections.Generic;
using System.Text;

namespace BL.DTOs.ExamSession
{
    public class AddExamSessionDto
    {
            public long SessionId { get; set; }

            public decimal? Score { get; set; }
            public int? TotalQuestions { get; set; }
            public bool? Passed { get; set; }

            public ICollection<AddSessionAnswerDto> SessionAnswers { get; set; } = new List<AddSessionAnswerDto>();
        
    }
}
