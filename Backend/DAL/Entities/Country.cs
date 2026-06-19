using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        // Navigation Properties
        public virtual ICollection<Governorate> Governorates { get; set; } = new List<Governorate>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
