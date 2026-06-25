namespace DAL.Entities
{
    public class AnswerChoice
    {
        public int ChoiceId { get; set; }
        public int AnswerId { get; set; }
        public char ChoiceKey { get; set; }
        public string ChoiceValue { get; set; }

        public virtual SessionAnswer SessionAnswer { get; set; }
    }
}
