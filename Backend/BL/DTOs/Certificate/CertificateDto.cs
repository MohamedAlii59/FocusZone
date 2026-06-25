using System;

namespace BL.DTOs.Certificate
{
    public class CertificateDto
    {
        public long CertificateId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Issuer { get; set; }
        public DateTime IssueDate { get; set; }
        public string Url { get; set; }
        public string CredentialId { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}