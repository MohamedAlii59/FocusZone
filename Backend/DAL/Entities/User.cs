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

        // Subscription and Payment Fields
        public DateTime? SubscriptionEndDate { get; set; }
        public int SessionMinutes { get; set; } = 300; // Free minutes for new users
        public bool IsPaidUser { get; set; } = false;
        public DateTime? LastMinutesResetDate { get; set; }

        // Audit Fields
        public string? CreatorUserId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifierUserId { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? DeleterUserId { get; set; }
        public DateTime? DeletedOn { get; set; }

        // Navigation Properties
         public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
         public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
         public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
         public virtual ICollection<UserTopicMastery> TopicMasteries { get; set; } = new List<UserTopicMastery>();
         public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
         public virtual ICollection<UserDomain> UserDomains { get; set; } = new List<UserDomain>();
         public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
         public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
         public virtual ICollection<Education> Educations { get; set; } = new List<Education>();
         public virtual ICollection<Experience> Experiences { get; set; } = new List<Experience>();
         public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
     }
 }
