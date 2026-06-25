using System;

namespace BL.DTOs.ExamSession
{
    public class StudySessionDto
    {
        public long SessionId { get; set; }
        public string UserId { get; set; }
        public long? ResourceId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string SessionSummary { get; set; }
        public int? DurationMinutes { get; set; }

        public ResourceBriefDto Resource { get; set; }
    }
}