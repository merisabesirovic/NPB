using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RideMateAPI.Data;
using RideMateAPI.DTOs;
using RideMateAPI.Models;

namespace RideMateAPI.Services
{
	public class UserService
	{
		private readonly RideMateDbContext _db;
		private readonly IPasswordHasher<User> _passwordHasher;

		public UserService(RideMateDbContext db, IPasswordHasher<User> passwordHasher)
		{
			_db = db;
			_passwordHasher = passwordHasher;
		}

		public async Task<UserProfileDto> GetProfileAsync(Guid userId)
		{
			var u = await _db.Users
				.Include(x => x.Vehicles)
				.Include(x => x.DriverVerificationDocuments)
				.FirstOrDefaultAsync(x => x.Id == userId);
			if (u == null) return null;
			return MapToProfileDto(u);
		}

		public async Task<UserProfileDto> GetUserProfilePublicAsync(Guid userId)
		{
			// same as GetProfileAsync for now but can hide sensitive fields
			return await GetProfileAsync(userId);
		}

		public async Task<List<UserProfileDto>> AdminGetAllAsync()
		{
			var users = await _db.Users
				.Include(u => u.Vehicles)
				.Include(u => u.DriverVerificationDocuments)
				.ToListAsync();
			return users.Select(MapToProfileDto).ToList();
		}

		public async Task<UserProfileDto> AdminCreateUserAsync(CreateUserAdminRequest req)
		{
			// Admins are only allowed to create other admins via this endpoint
			if (!req.IsAdmin) throw new ArgumentException("Admin may only create another admin via this endpoint");

			if (!RideMateAPI.Services.ValidationHelper.IsValidEmail(req.Email)) throw new ArgumentException("Invalid email format");
			var pwdError = RideMateAPI.Services.ValidationHelper.ValidatePassword(req.Password);
			if (pwdError != null) throw new ArgumentException(pwdError);

			var u = new User
			{
				Id = Guid.NewGuid(),
				Email = req.Email,
				FirstName = req.FirstName,
				LastName = req.LastName,
				DateOfBirth = DateTimeHelper.UtcMinValue,
				CreatedAt = DateTime.UtcNow,
				AvatarUrl = string.Empty,
				IsVerified = req.IsVerified,
				Roles = UserRole.Admin
			};
			u.PasswordHash = _passwordHasher.HashPassword(u, req.Password);
			_db.Users.Add(u);
			await _db.SaveChangesAsync();
			return await GetProfileAsync(u.Id);
		}

		private static UserProfileDto MapToProfileDto(User u)
		{
			return new UserProfileDto
			{
				Id = u.Id,
				FirstName = u.FirstName,
				LastName = u.LastName,
				Email = u.Email,
				AvatarUrl = u.AvatarUrl,
				AverageRating = u.AverageRating,
				TrustScore = u.TrustScore,
				CompletedRidesCount = u.CompletedRidesCount,
				CancelledRidesCount = u.CancelledRidesCount,
				Roles = (int)UserRoleHelper.EffectiveRoles(u),
				IsVerified = UserRoleHelper.HasApprovedIdentityVerification(u),
				IdentityVerificationPending = UserRoleHelper.HasPendingIdentityVerification(u),
				DriverVerificationPending = UserRoleHelper.HasPendingDriverVerification(u),
				DriverVerificationApproved = UserRoleHelper.HasApprovedDriverVerification(u),
				DriverVerificationStatus = UserRoleHelper.DriverVerificationStatus(u),
				Vehicles = u.Vehicles?.Select(v => new VehicleProfileDto
				{
					Id = v.Id,
					Model = v.Model,
					Year = v.Year,
					SeatsCount = v.SeatsCount,
					VehicleImageUrl = v.VehicleImageUrl,
					IsVerified = v.IsVerified
				}).ToList() ?? new List<VehicleProfileDto>()
			};
		}

		public async Task<bool> AdminDeleteUserAsync(Guid id)
		{
			var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
			if (u == null) return false;
			_db.Users.Remove(u);
			await _db.SaveChangesAsync();
			return true;
		}

		public async Task<bool> AdminChangePasswordAsync(Guid id, string newPassword)
		{
			var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
			if (u == null) return false;
			u.PasswordHash = _passwordHasher.HashPassword(u, newPassword);
			await _db.SaveChangesAsync();
			return true;
		}

		public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
		{
			var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
			if (u == null) return false;
			var verify = _passwordHasher.VerifyHashedPassword(u, u.PasswordHash, currentPassword);
			if (verify == PasswordVerificationResult.Failed) return false;
			u.PasswordHash = _passwordHasher.HashPassword(u, newPassword);
			await _db.SaveChangesAsync();
			return true;
		}
	}
}
