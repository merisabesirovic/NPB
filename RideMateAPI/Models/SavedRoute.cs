using System;

namespace RideMateAPI.Models
{
    public class SavedRoute
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string StartAddress { get; set; }
        public string DestinationAddress { get; set; }

        // Navigation
        public User User { get; set; }
    }
}
