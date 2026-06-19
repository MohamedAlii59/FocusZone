using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace DAL.Entities
{
    public class User : IdentityUser
    {
        // Personal Information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Mobile { get; set; }

        // Location Information
        public int? CountryId { get; set; }
        public Country Country { get; set; }

        public int? GovernorateId { get; set; }
        public Governorate Governorate { get; set; }

        public int? CityId { get; set; }
        public City City { get; set; }

        public string PostalCode { get; set; }

        // Interests
        public string InterestsToLearn { get; set; }

        // Audit Fields
        public string? CreatorUserId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifierUserId { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? DeleterUserId { get; set; }
        public DateTime? DeletedOn { get; set; }

        // Navigation Properties
        public virtual ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
    }
}
