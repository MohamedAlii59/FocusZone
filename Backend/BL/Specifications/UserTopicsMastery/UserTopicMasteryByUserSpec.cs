using BL.Pagination;
using DAL.Entities;


namespace BL.Specifications.UserTopicsMastery
{
    public class UserTopicMasteryByUserSpec : BaseSpecification<UserTopicMastery>
    {
        public UserTopicMasteryByUserSpec(string userId,PaginationParams pagination): base(x => x.UserId == userId)
        {
            AddInclude(x => x.Topic);

            AddOrderBy(x => x.Topic.Name);

            ApplyPaging((pagination.PageIndex - 1) * pagination.PageSize,pagination.PageSize);
        }
    }
}
