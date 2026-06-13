using System;

namespace RideMateAPI.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public decimal? RefundAmount { get; set; }
        public DateTime? RefundedAt { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Booking Booking { get; set; }
    }
}
