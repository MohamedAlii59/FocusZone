using System;

namespace DAL.Entities
{
    public class UserInterest
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Interest { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
    }
}
