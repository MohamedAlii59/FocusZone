using System;

namespace DAL.Entities
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GovernorateId { get; set; }

        // Navigation Properties
        public virtual Governorate Governorate { get; set; }
    }
}
