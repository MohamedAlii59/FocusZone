using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class UserDashboardDto
    {
        public int StudySessionsCount { get; set; }
        public int PassedExamsCount { get; set; }
        public int UserTopicMasteryCount { get; set; }
        public double PassRate { get; set; }
    }
}
