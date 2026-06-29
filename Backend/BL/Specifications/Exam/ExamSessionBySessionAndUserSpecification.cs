using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace BL.Specifications.Exam
{
    public class ExamSessionBySessionAndUserSpecification: BaseSpecification<ExamSession>
    {
        public ExamSessionBySessionAndUserSpecification(long sessionId,string userId) : base(x => x.SessionId == sessionId &&x.StudySession != null &&  x.StudySession.UserId == userId)
        {
            AddInclude(q => q.Include(x => x.StudySession).ThenInclude(x => x.Resource));

            AddInclude(q => q.Include(x => x.SessionAnswers).ThenInclude(x => x.AnswerChoices));
        }
    }
}
