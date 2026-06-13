using System;

namespace RideMateAPI.Models
{
    public class Vehicle
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int SeatsCount { get; set; }
        public string VehicleImageUrl { get; set; }
        public string LicenseNumber { get; set; }
        public string RegistrationCertificateUrl { get; set; }
        public bool IsVerified { get; set; }

        // Navigation
        public User User { get; set; }
    }
}
