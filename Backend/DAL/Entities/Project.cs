using System;

namespace DAL.Entities
{
    public class Project
    {
        public long ProjectId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Url { get; set; }
        public string? Role { get; set; }
        public string? Technologies { get; set; }

        public virtual User User { get; set; }
    }
}
