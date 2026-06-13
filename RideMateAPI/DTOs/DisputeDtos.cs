using System;

namespace RideMateAPI.DTOs
{
	public class CreateDisputeRequest
	{
		public Guid BookingId { get; set; }
		public string Description { get; set; }
	}

	public class DisputeDto
	{
		public Guid Id { get; set; }
		public Guid BookingId { get; set; }
		public Guid CreatedByUserId { get; set; }
		public string CreatedByEmail { get; set; }
		public string CreatedByFirstName { get; set; }
		public string CreatedByLastName { get; set; }
		public bool CreatedByIsVerified { get; set; }
		public string StartAddress { get; set; }
		public string DestinationAddress { get; set; }
		public string Description { get; set; }
		public string Status { get; set; }
		public string Resolution { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public class ChangeDisputeStatusRequest
	{
		public string Status { get; set; }
		public string Resolution { get; set; }
	}
}
