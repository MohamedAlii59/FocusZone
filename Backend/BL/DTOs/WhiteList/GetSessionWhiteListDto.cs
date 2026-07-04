using System;
using System.Collections.Generic;
using System.Text;

namespace BL.DTOs.WhiteList
{
    public class GetSessionWhiteListDto
    {
        public long SessionId { get; set; }
        public List<string> WhiteList { get; set; } = new List<string>();
    }
}
