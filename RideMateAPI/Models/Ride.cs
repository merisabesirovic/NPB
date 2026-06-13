using System;
using System.Collections.Generic;

namespace RideMateAPI.Models
{
    public class Ride
    {
        public Guid Id { get; set; }
        public Guid DriverId { get; set; }
        public bool IsRecurring { get; set; }
        public RecurringType RecurringType { get; set; }
        public DateTime? RecurringEndDate { get; set; }
        public string StartAddress { get; set; }
        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public string DestinationAddress { get; set; }
        // expected arrival / destination time
        public DateTime DestinationDateTime { get; set; }
        public double DestinationLatitude { get; set; }
        public double DestinationLongitude { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public int AvailableSeats { get; set; }
        public decimal PricePerSeat { get; set; }
        public RideType RideType { get; set; }
        public RideStatus RideStatus { get; set; }
        public bool AutoApproveBookings { get; set; }
        public bool SmokingAllowed { get; set; }
        public bool PetsAllowed { get; set; }
        public bool LuggageAllowed { get; set; }
        public bool MusicAllowed { get; set; }
        public bool ConversationAllowed { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public User Driver { get; set; }
        public ICollection<RideStop> RideStops { get; set; } = new List<RideStop>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
