using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Specifications.Exam
{
    public class PassedExamByUserCountSpecification: BaseSpecification<ExamSession>
    {
        public PassedExamByUserCountSpecification(string userId): base(x => x.Passed == true && x.StudySession != null && x.StudySession.UserId == userId)
        {
        }
    }
}
