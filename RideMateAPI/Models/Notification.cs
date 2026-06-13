using System;

namespace RideMateAPI.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public User User { get; set; }
    }
}
