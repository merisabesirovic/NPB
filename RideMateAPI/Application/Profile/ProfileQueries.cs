using MediatR;
using Microsoft.EntityFrameworkCore;
using RideMateAPI.Data;
using RideMateAPI.Models;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Profile
{
	public record GetProfileQuery(Guid? UserId, string? Email) : IRequest<object?>;

	public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, object?>
	{
		private readonly RideMateDbContext _db;

		public GetProfileQueryHandler(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<object?> Handle(GetProfileQuery request, CancellationToken cancellationToken)
		{
			var user = await ProfileQueryHelpers.FindUserAsync(_db, request.UserId, request.Email, cancellationToken);
			return user == null ? null : ProfileQueryHelpers.ToProfileResponse(user);
		}
	}

	public static class ProfileQueryHelpers
	{
		public static Task<User?> FindUserAsync(RideMateDbContext db, Guid? userId, string? email, CancellationToken cancellationToken)
		{
			var query = db.Users
				.Include(u => u.Vehicles)
				.Include(u => u.DriverVerificationDocuments)
				.AsQueryable();

			if (userId.HasValue)
			{
				return query.FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
			}

			if (!string.IsNullOrWhiteSpace(email))
			{
				var normalizedEmail = email.Trim().ToLower();
				return query.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
			}

			return Task.FromResult<User?>(null);
		}

		public static object ToProfileResponse(User user)
		{
			var roles = UserRoleHelper.EffectiveRoles(user);

			return new
			{
				user.Id,
				user.FirstName,
				user.LastName,
				user.Email,
				user.PhoneNumber,
				user.DateOfBirth,
				user.Biography,
				user.AvatarUrl,
				user.AverageRating,
				user.TrustScore,
				user.CompletedRidesCount,
				user.CancelledRidesCount,
				Roles = roles,
				IsVerified = UserRoleHelper.HasApprovedIdentityVerification(user),
				IdentityVerificationPending = UserRoleHelper.HasPendingIdentityVerification(user),
				DriverVerificationPending = UserRoleHelper.HasPendingDriverVerification(user),
				DriverVerificationApproved = UserRoleHelper.HasApprovedDriverVerification(user),
				DriverVerificationStatus = UserRoleHelper.DriverVerificationStatus(user),
				Vehicles = user.Vehicles.Select(v => new
				{
					v.Id,
					v.Model,
					v.Year,
					v.SeatsCount,
					v.VehicleImageUrl,
					v.LicenseNumber,
					v.RegistrationCertificateUrl,
					v.IsVerified
				})
			};
		}
	}
}
