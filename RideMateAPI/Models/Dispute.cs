using System;

namespace RideMateAPI.Models
{
    public class Dispute
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string Description { get; set; }
        public DisputeStatus Status { get; set; }
        public string Resolution { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Booking Booking { get; set; }
        public User CreatedByUser { get; set; }
    }
}
