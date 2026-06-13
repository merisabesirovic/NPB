using System;
using System.Collections.Generic;

namespace RideMateAPI.DTOs
{
	public class NotificationDto
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
		public bool IsRead { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public class CreateNotificationRequest
	{
		public Guid UserId { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
	}
}
