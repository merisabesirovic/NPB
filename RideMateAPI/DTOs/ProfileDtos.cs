using Microsoft.AspNetCore.Http;

namespace RideMateAPI.DTOs
{
	public class ProfileUpdateRequest
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? PhoneNumber { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string? Biography { get; set; }
		public IFormFile? Avatar { get; set; }
		public List<VehicleUpdate>? VehicleUpdates { get; set; }
	}

	public class VehicleUpdate
	{
		public Guid? Id { get; set; }
		public string? Model { get; set; }
		public int? Year { get; set; }
		public int? SeatsCount { get; set; }
		public string? LicenseNumber { get; set; }
		public IFormFile? VehicleImage { get; set; }
	}
}
