using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Bookings
{
	public record GetMyBookingsQuery(Guid UserId) : IRequest<List<BookingDto>>;
	public record GetDriverBookingsQuery(Guid DriverId) : IRequest<List<BookingDto>>;

	public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, List<BookingDto>>
	{
		private readonly BookingService _bookingService;

		public GetMyBookingsQueryHandler(BookingService bookingService)
		{
			_bookingService = bookingService;
		}

		public Task<List<BookingDto>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
		{
			return _bookingService.GetMyBookingsAsync(request.UserId);
		}
	}

	public class GetDriverBookingsQueryHandler : IRequestHandler<GetDriverBookingsQuery, List<BookingDto>>
	{
		private readonly BookingService _bookingService;

		public GetDriverBookingsQueryHandler(BookingService bookingService)
		{
			_bookingService = bookingService;
		}

		public Task<List<BookingDto>> Handle(GetDriverBookingsQuery request, CancellationToken cancellationToken)
		{
			return _bookingService.GetDriverBookingsAsync(request.DriverId);
		}
	}
}
