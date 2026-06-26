using System;
using System.Collections.Generic;
using System.Text;

namespace BL.DTOs.ExamSession
{
    public class AddSessionAnswerDto
    {
    
        public string Topic { get; set; }
        public string Question { get; set; }
        public string UserAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public string Explanation { get; set; }

        public ICollection<AddAnswerChoiceDto> AnswerChoices { get; set; } = new List<AddAnswerChoiceDto>();
    }
}
