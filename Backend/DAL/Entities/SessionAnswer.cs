using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class SessionAnswer
    {
        public int AnswerId { get; set; }
        public int SessionId { get; set; }
        public string Topic { get; set; }
        public string Question { get; set; }
        public string UserAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public string Explanation { get; set; }

        public virtual ExamSession ExamSession { get; set; }
        public virtual ICollection<AnswerChoice> AnswerChoices { get; set; } = new List<AnswerChoice>();
    }
}
