using System;

namespace RideMateAPI.DTOs
{
	public class CreateReviewRequest
	{
		public Guid BookingId { get; set; }
		public Guid ReviewedUserId { get; set; }
		public int Rating { get; set; }
		public string Comment { get; set; }
	}

	public class ReviewDto
	{
		public Guid Id { get; set; }
		public Guid BookingId { get; set; }
		public Guid ReviewerId { get; set; }
		public Guid ReviewedUserId { get; set; }
		public string ReviewerEmail { get; set; }
		public string ReviewerFirstName { get; set; }
		public string ReviewerLastName { get; set; }
		public bool ReviewerIsVerified { get; set; }
		public string ReviewedUserEmail { get; set; }
		public string ReviewedUserFirstName { get; set; }
		public string ReviewedUserLastName { get; set; }
		public bool ReviewedUserIsVerified { get; set; }
		public int Rating { get; set; }
		public string Comment { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
