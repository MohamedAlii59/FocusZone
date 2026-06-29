using BL.DTOs.ExamSession;
using BL.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Services.Abstraction
{
    public interface IExamSessionService
    {
        Task<PaginationResponse<ExamSessionDto>> GetByUserAsync( string userId,PaginationParams pagination, string? search = null,string? type=null);


        Task<ExamSessionDto?> GetAsync(int id);

        Task<ExamSessionDto> CreateAsync(AddExamSessionDto dto);

        Task<ExamSessionDto?> GetBySessionAndUserAsync( long sessionId,   string userId);

        Task UpdateAsync(int id, ExamSessionDto dto);

        Task DeleteAsync(int id);
    }
}
