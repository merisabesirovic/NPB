using Microsoft.AspNetCore.Http;
using RideMateAPI.Models;

namespace RideMateAPI.DTOs
{
	public class RegisterRequest
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string PhoneNumber { get; set; } = string.Empty;
		public DateTime? DateOfBirth { get; set; }
		public string Biography { get; set; } = string.Empty;
		public IFormFile? Avatar { get; set; }
		public bool RegisterAsDriver { get; set; }
		public string? DriverLicenseNumber { get; set; }
		public IFormFile? IdentityDocumentFile { get; set; }
		public string? VehicleModel { get; set; }
		public int? VehicleYear { get; set; }
		public int? VehicleSeats { get; set; }
	}

	public class RegisterResponse
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = string.Empty;
	}

	public class LoginRequest
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}

	public class AuthResponse
	{
		public string Token { get; set; } = string.Empty;
		public DateTime TokenExpiresAt { get; set; }
		public string RefreshToken { get; set; } = string.Empty;
		public DateTime RefreshTokenExpiresAt { get; set; }
		public bool DriverVerificationPending { get; set; }
	}

	public class RefreshTokenRequest
	{
		public string RefreshToken { get; set; } = string.Empty;
	}

	public class RevokeRefreshTokenRequest
	{
		public string RefreshToken { get; set; } = string.Empty;
	}

	public class AuthUserResponse
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public UserRole Roles { get; set; }
		public bool DriverVerificationPending { get; set; }
	}
}
