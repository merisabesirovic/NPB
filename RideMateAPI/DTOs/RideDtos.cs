using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace RideMateAPI.DTOs
{
	public class CreateRideDto
	{
		public string StartAddress { get; set; }
		public double StartLatitude { get; set; }
		public double StartLongitude { get; set; }
		public string DestinationAddress { get; set; }
		public double DestinationLatitude { get; set; }
		public double DestinationLongitude { get; set; }
		public DateTime DepartureDateTime { get; set; }
		public DateTime DestinationDateTime { get; set; }
		public int AvailableSeats { get; set; }
		public decimal PricePerSeat { get; set; }
		public string RideType { get; set; }
		public bool AutoApproveBookings { get; set; }
		public bool SmokingAllowed { get; set; }
		public bool PetsAllowed { get; set; }
		public bool LuggageAllowed { get; set; }
		public bool MusicAllowed { get; set; }
		public bool ConversationAllowed { get; set; }
	}

	public class RideDto
	{
		public Guid Id { get; set; }
		public Guid DriverId { get; set; }
		public string DriverEmail { get; set; }
		public bool DriverIsVerified { get; set; }
		public string StartAddress { get; set; }
		public double StartLatitude { get; set; }
		public double StartLongitude { get; set; }
		public string DestinationAddress { get; set; }
		public double DestinationLatitude { get; set; }
		public double DestinationLongitude { get; set; }
		public DateTime DepartureDateTime { get; set; }
		public DateTime DestinationDateTime { get; set; }
		public int AvailableSeats { get; set; }
		public decimal PricePerSeat { get; set; }
		public string RideType { get; set; }
		public string RideStatus { get; set; }
		public bool AutoApproveBookings { get; set; }
		public bool SmokingAllowed { get; set; }
		public bool PetsAllowed { get; set; }
		public bool LuggageAllowed { get; set; }
		public bool MusicAllowed { get; set; }
		public bool ConversationAllowed { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public class UpdateRideDto
	{
		public string StartAddress { get; set; }
		public double? StartLatitude { get; set; }
		public double? StartLongitude { get; set; }
		public string DestinationAddress { get; set; }
		public double? DestinationLatitude { get; set; }
		public double? DestinationLongitude { get; set; }
		public DateTime? DepartureDateTime { get; set; }
		public int? AvailableSeats { get; set; }
		public decimal? PricePerSeat { get; set; }
		public string RideType { get; set; }
		public bool? AutoApproveBookings { get; set; }
		public bool? SmokingAllowed { get; set; }
		public bool? PetsAllowed { get; set; }
		public bool? LuggageAllowed { get; set; }
		public bool? MusicAllowed { get; set; }
		public bool? ConversationAllowed { get; set; }
	}

	public class SearchRideRequest
	{
		public string? StartAddress { get; set; }
		public string? DestinationAddress { get; set; }

		// Start location center
		public double? StartLatitude { get; set; }
		public double? StartLongitude { get; set; }
		// radius in kilometers
		public double? StartRadiusKm { get; set; }

		// Destination location center
		public double? DestinationLatitude { get; set; }
		public double? DestinationLongitude { get; set; }
		public double? DestinationRadiusKm { get; set; }

		// Date only filter - matches any time during that date (UTC)
		public DateTime? DepartureDate { get; set; }

		public decimal? MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }
		public int? MinAvailableSeats { get; set; }

		// Pagination
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;

		// Debug - when set true server will return counts to help troubleshoot searches
		public bool Debug { get; set; } = false;
	}

	public class RideListResponse
	{
		public int TotalCount { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
		public List<RideDto> Items { get; set; } = new List<RideDto>();

		// Optional debug information returned when Debug flag is set on the request
		public SearchDebugInfo DebugInfo { get; set; }
	}

	public class SearchDebugInfo
	{
		public int DbCandidates { get; set; }
		public int ValidCandidates { get; set; }
		public int AvailableRides { get; set; }
		public bool DateFilterApplied { get; set; }
		public bool PriceFilterApplied { get; set; }
	}

	public class ChangeRideStatusRequest
	{
		// Status as string name of RideStatus enum, e.g. "InProgress"
		public string Status { get; set; }
	}
}

