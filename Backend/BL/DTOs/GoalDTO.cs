using System;
using System.Collections.Generic;
using System.Text;

namespace BL.DTOs
{
    public class GoalDTO
    {
        public long GoalId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int? Priority { get; set; } = 1;
        public string? Category { get; set; } // Can be used to distinguish between interests and goals
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
