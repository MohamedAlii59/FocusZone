using System;

namespace DAL.Entities
{
    public class Experience
    {
        public long ExperienceId { get; set; }
        public string UserId { get; set; }
        public string Company { get; set; }
        public string Role { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public bool? Current { get; set; }
        public int? SortOrder { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }

        public virtual User User { get; set; }
    }
}
