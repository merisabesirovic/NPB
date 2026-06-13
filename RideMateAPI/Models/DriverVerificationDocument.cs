using System;

namespace RideMateAPI.Models
{
    public class DriverVerificationDocument
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FileUrl { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public DateTime UploadedAt { get; set; }

        // Navigation
        public User User { get; set; }
    }
}
