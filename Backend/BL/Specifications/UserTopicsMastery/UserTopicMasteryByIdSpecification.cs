using DAL.Entities;

namespace BL.Specifications.UserTopicsMastery
{
    public class UserTopicMasteryByIdSpecification : BaseSpecification<UserTopicMastery>
    {
        public UserTopicMasteryByIdSpecification(string userId, int topicId): base(x => x.UserId == userId && x.TopicId == topicId)
        {
            AddInclude(x => x.Topic);
        }
    }
}
