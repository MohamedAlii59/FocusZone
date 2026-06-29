using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BL.Specifications.Exam
{
    public class ExamSessionByIdSpecification : BaseSpecification<ExamSession>
    {
        public ExamSessionByIdSpecification(int examId): base(x => x.ExamId == examId)
        {
            AddInclude(q => q.Include(x => x.StudySession).ThenInclude(x => x.Resource));

            AddInclude(q => q.Include(x => x.SessionAnswers).ThenInclude(x => x.AnswerChoices));
        }
    }
}
