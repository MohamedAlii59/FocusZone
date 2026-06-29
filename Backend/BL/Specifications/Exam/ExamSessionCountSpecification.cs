using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Specifications.Exam
{
    
        public class ExamSessionCountSpecification: BaseSpecification<ExamSession>
        {
            public ExamSessionCountSpecification( string userId, string? search,string? type): base(x =>x.StudySession != null &&   x.StudySession.UserId == userId &&

                    (string.IsNullOrWhiteSpace(search) ||
                     x.StudySession.Resource.Title.Contains(search))

                    &&

                    (string.IsNullOrWhiteSpace(type) ||
                     x.StudySession.Resource.Type == type))
            {

            }
        }
    
}
