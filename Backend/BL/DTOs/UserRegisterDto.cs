using System;

namespace BL.DTOs
{
    public class UserRegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Mobile { get; set; }
        public int? CountryId { get; set; }
        public int? GovernorateId { get; set; }
        public int? CityId { get; set; }
        public string PostalCode { get; set; }
        public string InterestsToLearn { get; set; }
    }
}
