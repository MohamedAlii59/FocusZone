using System.Collections.Generic;

namespace BL.DTOs.ExamSession
{
    public class SessionAnswerDto
    {
        public int AnswerId { get; set; }
        public int ExamId { get; set; }
        public string Topic { get; set; }
        public string Question { get; set; }
        public string UserAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public string Explanation { get; set; }

        public ICollection<AnswerChoiceDto> AnswerChoices { get; set; } = new List<AnswerChoiceDto>();
    }
}