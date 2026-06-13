using AutoMapper;
using RideMateAPI.DTOs;
using RideMateAPI.Models;
using RideMateAPI.Services;

namespace RideMateAPI.Mapping
{
	public class RideMateMappingProfile : Profile
	{
		public RideMateMappingProfile()
		{
			CreateMap<RegisterRequest, User>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
				.ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName ?? string.Empty))
				.ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName ?? string.Empty))
				.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
				.ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber ?? string.Empty))
				.ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => DateTimeHelper.DateOnlyAsUtc(src.DateOfBirth)))
				.ForMember(dest => dest.Biography, opt => opt.MapFrom(src => src.Biography ?? string.Empty))
				.ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(_ => string.Empty))
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
				.ForMember(dest => dest.Roles, opt => opt.MapFrom(_ => UserRole.Passenger))
				.ForMember(dest => dest.IsVerified, opt => opt.MapFrom(_ => false))
				.ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
				.ForMember(dest => dest.Vehicles, opt => opt.Ignore())
				.ForMember(dest => dest.Rides, opt => opt.Ignore())
				.ForMember(dest => dest.Bookings, opt => opt.Ignore())
				.ForMember(dest => dest.SavedRoutes, opt => opt.Ignore())
				.ForMember(dest => dest.Notifications, opt => opt.Ignore())
				.ForMember(dest => dest.RefreshTokens, opt => opt.Ignore())
				.ForMember(dest => dest.DriverVerificationDocuments, opt => opt.Ignore())
				.ForMember(dest => dest.ReviewsWritten, opt => opt.Ignore())
				.ForMember(dest => dest.ReviewsReceived, opt => opt.Ignore())
				.ForMember(dest => dest.Disputes, opt => opt.Ignore());

			CreateMap<RegisterRequest, Vehicle>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
				.ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.DriverLicenseNumber ?? string.Empty))
				.ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.VehicleModel ?? string.Empty))
				.ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.VehicleYear ?? 0))
				.ForMember(dest => dest.SeatsCount, opt => opt.MapFrom(src => src.VehicleSeats ?? 4))
				.ForMember(dest => dest.VehicleImageUrl, opt => opt.MapFrom(_ => string.Empty))
				.ForMember(dest => dest.RegistrationCertificateUrl, opt => opt.MapFrom(_ => string.Empty))
				.ForMember(dest => dest.IsVerified, opt => opt.MapFrom(_ => false))
				.ForMember(dest => dest.UserId, opt => opt.Ignore())
				.ForMember(dest => dest.User, opt => opt.Ignore());

			CreateMap<User, AuthUserResponse>()
				.ForMember(dest => dest.Roles, opt => opt.MapFrom(src => UserRoleHelper.EffectiveRoles(src)))
				.ForMember(dest => dest.DriverVerificationPending, opt => opt.MapFrom(src => UserRoleHelper.HasPendingDriverVerification(src)));
		}
	}
}
