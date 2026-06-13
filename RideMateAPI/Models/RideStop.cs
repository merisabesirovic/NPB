using System;

namespace RideMateAPI.Models
{
    public class RideStop
    {
        public Guid Id { get; set; }
        public Guid RideId { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Order { get; set; }

        // Navigation
        public Ride Ride { get; set; }
    }
}
