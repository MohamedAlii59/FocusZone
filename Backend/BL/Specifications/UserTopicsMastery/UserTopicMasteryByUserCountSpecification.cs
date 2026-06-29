using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Specifications.UserTopicsMastery
{
    public class UserTopicMasteryByUserCountSpecification: BaseSpecification<UserTopicMastery>
    {
        public UserTopicMasteryByUserCountSpecification(string userId) : base(x => x.UserId == userId)
        {
        }
    }
}
