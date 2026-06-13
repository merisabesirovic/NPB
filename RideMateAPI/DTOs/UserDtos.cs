using System;
using System.Collections.Generic;

namespace RideMateAPI.DTOs
{
	public class VehicleProfileDto
	{
		public Guid Id { get; set; }
		public string Model { get; set; }
		public int Year { get; set; }
		public int SeatsCount { get; set; }
		public string VehicleImageUrl { get; set; }
		public bool IsVerified { get; set; }
	}

	public class UserProfileDto
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string AvatarUrl { get; set; }
		public double AverageRating { get; set; }
		public double TrustScore { get; set; }
		public int CompletedRidesCount { get; set; }
		public int CancelledRidesCount { get; set; }
		public int Roles { get; set; }
		public bool IsVerified { get; set; }
		public bool IdentityVerificationPending { get; set; }
		public bool DriverVerificationPending { get; set; }
		public bool DriverVerificationApproved { get; set; }
		public string DriverVerificationStatus { get; set; } = string.Empty;
		public List<VehicleProfileDto> Vehicles { get; set; } = new List<VehicleProfileDto>();
	}

	public class CreateUserAdminRequest
	{
		// Admin may only create another admin. Set IsAdmin = true to create an admin user.
		public string Email { get; set; }
		public string Password { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public bool IsAdmin { get; set; } = true;
		public bool IsVerified { get; set; } = true;
	}

	public class ChangePasswordRequest
	{
		public string CurrentPassword { get; set; }
		public string NewPassword { get; set; }
	}

	public class AdminChangePasswordRequest
	{
		public string NewPassword { get; set; }
	}
}
