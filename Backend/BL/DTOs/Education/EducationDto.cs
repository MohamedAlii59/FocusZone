using System;

namespace BL.DTOs.Education
{
    public class EducationDto
    {
        public long EducationId { get; set; }
        public string UserId { get; set; }
        public string Institution { get; set; }
        public string Degree { get; set; }
        public string Field { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}