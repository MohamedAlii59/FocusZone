using DAL.Entities;

namespace BL.Specifications.Exam
{
    public class ActiveStudySessionSpecification : BaseSpecification<StudySession>
    {
        public ActiveStudySessionSpecification(string userId): base(x => x.UserId == userId && x.EndedAt == null)
        {
            AddOrderByDescending(x => x.StartedAt);
        }
    }
}