namespace BL.DTOs.ExamSession
{
    public class AnswerChoiceDto
    {
        public int ChoiceId { get; set; }
        public int AnswerId { get; set; }
        public char ChoiceKey { get; set; }
        public string ChoiceValue { get; set; }
    }
}