using FluentValidation;
using RideMateAPI.DTOs;

namespace RideMateAPI.Validation
{
	public class CreateRideDtoValidator : AbstractValidator<CreateRideDto>
	{
		public CreateRideDtoValidator()
		{
			RuleFor(x => x.StartAddress).NotEmpty().MaximumLength(500);
			RuleFor(x => x.DestinationAddress).NotEmpty().MaximumLength(500);
			RuleFor(x => x.DepartureDateTime).Must(value => value > DateTime.UtcNow.AddMinutes(30)).WithMessage("Departure must be at least 30 minutes in the future");
			RuleFor(x => x.DestinationDateTime).GreaterThan(x => x.DepartureDateTime).WithMessage("Destination time must be after departure time");
			RuleFor(x => x.AvailableSeats).GreaterThan(0);
			RuleFor(x => x.PricePerSeat).GreaterThan(0);
		}
	}
}
