using System;
using System.Collections.Generic;

namespace RideMateAPI.DTOs
{
	public class CreateBookingRequest
	{
		public Guid RideId { get; set; }
		// Optional: for one-time rides the server will use the ride's DepartureDateTime.
		// For recurring rides the passenger must provide RideDate.
		public DateTime? RideDate { get; set; }
		public int SeatsReserved { get; set; }
		public string PickupPoint { get; set; }
		public string DropoffPoint { get; set; }
		public string Note { get; set; }
		// Payment options
		public string PaymentMethod { get; set; } // "Cash" or "Online"
		public decimal? PaidAmount { get; set; } // amount paid now (for online)
	}

	public class UpdateBookingRequest
	{
		public string Note { get; set; }
	}

	public class MockOnlinePaymentRequest
	{
		public string CardholderName { get; set; }
		public string CardNumber { get; set; }
		public string Expiry { get; set; }
		public string Cvv { get; set; }
	}

	public class BookingDto
	{
		public Guid Id { get; set; }
		public Guid RideId { get; set; }
		// The scheduled date for the ride
		public DateTime RideDate { get; set; }
		public Guid PassengerId { get; set; }
		public int SeatsReserved { get; set; }
		public string PickupPoint { get; set; }
		public string DropoffPoint { get; set; }
		public string Note { get; set; }
		public string BookingStatus { get; set; }
		public decimal TotalPrice { get; set; }
		public DateTime CreatedAt { get; set; }
		public string PaymentMethod { get; set; } // "Cash" or "Online"
		public string PaymentStatus { get; set; } // e.g. "Pending", "Paid", "Refunded"
		public decimal? PaidAmount { get; set; }
		public decimal? RefundAmount { get; set; }
		public Guid DriverId { get; set; }
		public string DriverEmail { get; set; }
		public string DriverFirstName { get; set; }
		public string DriverLastName { get; set; }
		public bool DriverIsVerified { get; set; }
		public string PassengerEmail { get; set; }
		public string PassengerFirstName { get; set; }
		public string PassengerLastName { get; set; }
		public bool PassengerIsVerified { get; set; }
		public string StartAddress { get; set; }
		public string DestinationAddress { get; set; }
		public DateTime DepartureDateTime { get; set; }
		public string RideStatus { get; set; }
	}

	public class ChangeBookingStatusRequest
	{
		public string Status { get; set; }
	}
}
