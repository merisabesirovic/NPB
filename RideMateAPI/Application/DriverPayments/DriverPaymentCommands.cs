using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.DriverPayments
{
	public record MarkCashPaidCommand(Guid DriverId, Guid BookingId) : IRequest<BookingDto?>;

	public class MarkCashPaidCommandHandler : IRequestHandler<MarkCashPaidCommand, BookingDto?>
	{
		private readonly BookingService _bookingService;

		public MarkCashPaidCommandHandler(BookingService bookingService)
		{
			_bookingService = bookingService;
		}

		public Task<BookingDto?> Handle(MarkCashPaidCommand request, CancellationToken cancellationToken)
		{
			return _bookingService.MarkCashPaidAsync(request.DriverId, request.BookingId);
		}
	}
}
