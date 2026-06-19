using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class Governorate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }

        // Navigation Properties
        public virtual Country Country { get; set; }
        public virtual ICollection<City> Cities { get; set; } = new List<City>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
