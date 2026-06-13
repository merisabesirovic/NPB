using FluentValidation;
using RideMateAPI.Application.Auth;
using RideMateAPI.DTOs;

namespace RideMateAPI.Validation
{
	public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
	{
		public RegisterRequestValidator()
		{
			RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
			RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
			RuleFor(x => x.Email).NotEmpty().EmailAddress();
			RuleFor(x => x.Password)
				.NotEmpty()
				.MinimumLength(8)
				.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
				.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
				.Matches("[0-9]").WithMessage("Password must contain at least one digit")
				.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
			RuleFor(x => x.PhoneNumber).MaximumLength(50);
			RuleFor(x => x.Biography).MaximumLength(2000);
			RuleFor(x => x.DriverLicenseNumber)
				.NotEmpty()
				.When(x => x.RegisterAsDriver)
				.WithMessage("Driver registration requires DriverLicenseNumber");
			RuleFor(x => x.IdentityDocumentFile)
				.NotNull()
				.When(x => x.RegisterAsDriver)
				.WithMessage("Driver registration requires identity document upload");
			RuleFor(x => x.VehicleSeats)
				.GreaterThan(0)
				.When(x => x.VehicleSeats.HasValue);
		}
	}

	public class LoginRequestValidator : AbstractValidator<LoginRequest>
	{
		public LoginRequestValidator()
		{
			RuleFor(x => x.Email).NotEmpty().EmailAddress();
			RuleFor(x => x.Password).NotEmpty();
		}
	}

	public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
	{
		public RefreshTokenRequestValidator()
		{
			RuleFor(x => x.RefreshToken).NotEmpty();
		}
	}

	public class RevokeRefreshTokenRequestValidator : AbstractValidator<RevokeRefreshTokenRequest>
	{
		public RevokeRefreshTokenRequestValidator()
		{
			RuleFor(x => x.RefreshToken).NotEmpty();
		}
	}

	public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
	{
		public RegisterCommandValidator()
		{
			RuleFor(x => x.Request).SetValidator(new RegisterRequestValidator());
		}
	}

	public class LoginCommandValidator : AbstractValidator<LoginCommand>
	{
		public LoginCommandValidator()
		{
			RuleFor(x => x.Request).SetValidator(new LoginRequestValidator());
		}
	}

	public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
	{
		public RefreshTokenCommandValidator()
		{
			RuleFor(x => x.Request).SetValidator(new RefreshTokenRequestValidator());
		}
	}

	public class RevokeRefreshTokenCommandValidator : AbstractValidator<RevokeRefreshTokenCommand>
	{
		public RevokeRefreshTokenCommandValidator()
		{
			RuleFor(x => x.Request).SetValidator(new RevokeRefreshTokenRequestValidator());
		}
	}
}
