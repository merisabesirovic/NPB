using MediatR;
using Microsoft.EntityFrameworkCore;
using RideMateAPI.Data;
using RideMateAPI.DTOs;
using RideMateAPI.Models;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Profile
{
	public record UpdateProfileCommand(Guid? UserId, string? Email, ProfileUpdateRequest Request) : IRequest<bool>;

	public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
	{
		private readonly RideMateDbContext _db;
		private readonly CloudinaryService _cloudinary;

		public UpdateProfileCommandHandler(RideMateDbContext db, CloudinaryService cloudinary)
		{
			_db = db;
			_cloudinary = cloudinary;
		}

		public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
		{
			var user = await FindUserForProfileUpdateAsync(request.UserId, request.Email, cancellationToken);
			if (user == null)
			{
				return false;
			}

			var req = request.Request;
			if (!string.IsNullOrWhiteSpace(req.FirstName)) user.FirstName = req.FirstName;
			if (!string.IsNullOrWhiteSpace(req.LastName)) user.LastName = req.LastName;
			if (!string.IsNullOrWhiteSpace(req.PhoneNumber)) user.PhoneNumber = req.PhoneNumber;
			if (!string.IsNullOrWhiteSpace(req.Biography)) user.Biography = req.Biography;
			if (req.DateOfBirth.HasValue) user.DateOfBirth = DateTimeHelper.DateOnlyAsUtc(req.DateOfBirth);

			if (req.Avatar != null && req.Avatar.Length > 0)
			{
				user.AvatarUrl = await _cloudinary.UploadAsync(req.Avatar, folder: "avatars");
			}

			if (req.VehicleUpdates != null && req.VehicleUpdates.Any())
			{
				if (!UserRoleHelper.EffectiveRoles(user).HasFlag(UserRole.Driver))
				{
					throw new InvalidOperationException("Driver profile is waiting for admin approval");
				}

				foreach (var vehicleUpdate in req.VehicleUpdates)
				{
					if (vehicleUpdate.Id != null && vehicleUpdate.Id != Guid.Empty)
					{
						var vehicle = await _db.Vehicles
							.FirstOrDefaultAsync(v => v.Id == vehicleUpdate.Id.Value && v.UserId == user.Id, cancellationToken);
						if (vehicle == null)
						{
							throw new KeyNotFoundException("Vehicle not found");
						}

						if (!string.IsNullOrWhiteSpace(vehicleUpdate.Model)) vehicle.Model = vehicleUpdate.Model;
						if (vehicleUpdate.Year.HasValue) vehicle.Year = vehicleUpdate.Year.Value;
						if (vehicleUpdate.SeatsCount.HasValue) vehicle.SeatsCount = vehicleUpdate.SeatsCount.Value;
						if (!string.IsNullOrWhiteSpace(vehicleUpdate.LicenseNumber)) vehicle.LicenseNumber = vehicleUpdate.LicenseNumber;
						if (vehicleUpdate.VehicleImage != null && vehicleUpdate.VehicleImage.Length > 0)
						{
							vehicle.VehicleImageUrl = await _cloudinary.UploadAsync(vehicleUpdate.VehicleImage, folder: "vehicle_images");
						}
					}
					else
					{
						var newVehicle = new Vehicle
						{
							Id = Guid.NewGuid(),
							UserId = user.Id,
							Model = vehicleUpdate.Model ?? string.Empty,
							Year = vehicleUpdate.Year ?? 0,
							SeatsCount = vehicleUpdate.SeatsCount ?? 4,
							VehicleImageUrl = string.Empty,
							LicenseNumber = vehicleUpdate.LicenseNumber ?? string.Empty,
							RegistrationCertificateUrl = string.Empty,
							IsVerified = false
						};

						if (vehicleUpdate.VehicleImage != null && vehicleUpdate.VehicleImage.Length > 0)
						{
							newVehicle.VehicleImageUrl = await _cloudinary.UploadAsync(vehicleUpdate.VehicleImage, folder: "vehicle_images");
						}

						_db.Vehicles.Add(newVehicle);
					}
				}
			}

			await _db.SaveChangesAsync(cancellationToken);
			return true;
		}

		private Task<User?> FindUserForProfileUpdateAsync(Guid? userId, string? email, CancellationToken cancellationToken)
		{
			var query = _db.Users
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
	}
}
