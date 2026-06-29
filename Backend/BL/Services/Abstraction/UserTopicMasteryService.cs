using BL.DTOs.UserTopicMastery;
using BL.Pagination;


namespace BL.Services.Abstraction
{
    public interface IUserTopicMasteryService
    {
        Task<PaginationResponse<UserTopicMasteryDto>> GetByUserAsync(string userId, PaginationParams pagination);

        Task CreateAsync(UserTopicMasteryDto dto);

        Task UpdateAsync(UserTopicMasteryDto dto);

        Task DeleteAsync(string userId, int topicId);
    }
}
