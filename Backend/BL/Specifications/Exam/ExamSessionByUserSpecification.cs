using BL.Pagination;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BL.Specifications.Exam
{
    public class ExamSessionByUserSpecification: BaseSpecification<ExamSession>
    {
       
        public ExamSessionByUserSpecification( string userId, PaginationParams pagination,string? search,string? type)  : 
            base(x => x.StudySession != null && x.StudySession.UserId == userId && (string.IsNullOrWhiteSpace(search) ||x.StudySession.Resource.Title.Contains(search))
            &&(string.IsNullOrWhiteSpace(type) || x.StudySession.Resource.Type == type))
        {
            AddInclude(q => q
                .Include(x => x.StudySession)
                .ThenInclude(x => x.Resource));

            AddInclude(q => q
                .Include(x => x.SessionAnswers)
                .ThenInclude(x => x.AnswerChoices));

            AddOrderByDescending(x => x.StudySession!.EndedAt);

            ApplyPaging((pagination.PageIndex - 1) * pagination.PageSize, pagination.PageSize);
        }
    
    }
}
