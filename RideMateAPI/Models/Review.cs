using System;

namespace RideMateAPI.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid ReviewerId { get; set; }
        public Guid ReviewedUserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Booking Booking { get; set; }
        public User Reviewer { get; set; }
        public User ReviewedUser { get; set; }
    }
}
