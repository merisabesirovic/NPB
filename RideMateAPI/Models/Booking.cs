using System;
using System.Collections.Generic;

namespace RideMateAPI.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid RideId { get; set; }
        public DateTime RideDate { get; set; }
        public Guid PassengerId { get; set; }
        public int SeatsReserved { get; set; }
        public string PickupPoint { get; set; }
        public string DropoffPoint { get; set; }
        public string Note { get; set; } = string.Empty;
        public BookingStatus BookingStatus { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Ride Ride { get; set; }
        public User Passenger { get; set; }
        public Payment Payment { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
    }
}
