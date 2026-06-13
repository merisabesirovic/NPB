using System;
using System.Collections.Generic;

namespace RideMateAPI.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
        public string Biography { get; set; }
        public string AvatarUrl { get; set; }
        public double AverageRating { get; set; }
        public double TrustScore { get; set; }
        public int CompletedRidesCount { get; set; }
        public int CancelledRidesCount { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserRole Roles { get; set; }

        // Navigation properties
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<Ride> Rides { get; set; } = new List<Ride>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<SavedRoute> SavedRoutes { get; set; } = new List<SavedRoute>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<DriverVerificationDocument> DriverVerificationDocuments { get; set; } = new List<DriverVerificationDocument>();
        public ICollection<Review> ReviewsWritten { get; set; } = new List<Review>();
        public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
        public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
    }
}
