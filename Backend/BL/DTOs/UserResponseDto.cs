using System;
using System.Collections.Generic;
using BL.DTOs.Certificate;
using BL.DTOs.Education;
using BL.DTOs.Experience;
using BL.DTOs.Project;
using BL.DTOs.UserTopicMastery;

namespace BL.DTOs
{
    public class UserResponseDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Mobile { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public int? GovernorateId { get; set; }
        public string GovernorateName { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string PostalCode { get; set; }
        public string InterestsToLearn { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifierUserId { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public string DeleterUserId { get; set; }
        public DateTime? DeletedOn { get; set; }
        
        // Subscription and Payment Fields
        public DateTime? SubscriptionEndDate { get; set; }
        public int SessionMinutes { get; set; }
        public bool IsPaidUser { get; set; }
        public string[] Roles { get; set; }

        public ICollection<CertificateDto> Certificates { get; set; } = new List<CertificateDto>();
        public ICollection<EducationDto> Educations { get; set; } = new List<EducationDto>();
        public ICollection<ExperienceDto> Experiences { get; set; } = new List<ExperienceDto>();
        public ICollection<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
        public ICollection<UserTopicMasteryDto> UserTopicMasteries { get; set; } = new List<UserTopicMasteryDto>();
    }
}
