using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DAL.Entities
{
    public class SessionExamActive
    {
        public long SessionExamActiveId { get; set; } // PK
        public long SessionId { get; set; } // FK to StudySession.SessionId
        public bool IsActive { get; set; } // Indicates if the exam session is currently active

        [ForeignKey("SessionId")]
        public virtual StudySession? StudySession { get; set; }
    }
}
