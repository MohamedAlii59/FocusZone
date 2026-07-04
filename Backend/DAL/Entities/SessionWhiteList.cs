using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DAL.Entities
{
    public class SessionWhiteList
    {
        [Key]
        public long SessionWhiteListId { get; set; } // PK
        public long SessionId { get; set; } // FK to StudySession.SessionId
        public string Url { get; set; } // The URL that is whitelisted for the user in this session

        [ForeignKey("SessionId")]
        public virtual StudySession? StudySession { get; set; }
    }
}
