using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Bookings
{
	public record CreateBookingCommand(Guid PassengerId, CreateBookingRequest Request) : IRequest<BookingDto>;
	public record ApproveBookingCommand(Guid DriverId, Guid BookingId) : IRequest<BookingDto?>;
	public record RejectBookingCommand(Guid DriverId, Guid BookingId) : IRequest<BookingDto?>;
	public record CancelBookingCommand(Guid UserId, Guid BookingId) : IRequest<BookingDto?>;
	public record UpdateBookingCommand(Guid PassengerId, Guid BookingId, UpdateBookingRequest Request) : IRequest<BookingDto?>;
	public record PayOnlineCommand(Guid PassengerId, Guid BookingId, MockOnlinePaymentRequest Request) : IRequest<BookingDto?>;

	public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingDto>
	{
		private readonly BookingService _bookingService;

		public CreateBookingCommandHandler(BookingService bookingService)
		{
			_bookingService = bookingService;
		}

		public Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
		{
			return _bookingService.CreateBookingAsync(request.PassengerId, request.Request);
		}
	}

	public class ApproveBookingCommandHandler : IRequestHandler<ApproveBookingCommand, BookingDto?>
	{
		private readonly BookingService _bookingService;

		public ApproveBookingCommandHandler(BookingService bookingService)
		{
			_bookingService = bookingService;
		}

		public Task<BookingDto?> Handle(ApproveBookingCommand request, CancellationToken cancellationToken)
		{
			return _bookingService.ApproveBookingAsync(request.DriverId, request.BookingId);
		}
	}

	public class RejectBookingCommandHandler : IRequestHandler<RejectBookingCommand, BookingDto?>
	{
		private readonly BookingService _bookingService;

		public RejectBookingCommandHandler(BookingService bookingService)
		{
			_bookingService = bookingService;
		}

		public Task<BookingDto?> Handle(RejectBookingCommand request, CancellationToken cancellationToken)
		{
			return _bookingService.RejectBookingAsync(request.DriverId, request.BookingId);
		}
	}

	public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, BookingDto?>
	{
		private readonly BookingService _bookingService;

		public CancelBookingCommandHandler(BookingService bookingService)
		{
			_bookingService = bookingService;
		}

		public Task<BookingDto?> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
		{
			return _bookingService.CancelBookingAsync(request.UserId, request.BookingId);
		}
	}

	public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand, BookingDto?>
	{
		private readonly BookingService _bookingService;

		public UpdateBookingCommandHandler(BookingService bookingService)
		{
			_bookingService = bookingService;
		}

		public Task<BookingDto?> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
		{
			return _bookingService.UpdateBookingAsync(request.PassengerId, request.BookingId, request.Request);
		}
	}

	public class PayOnlineCommandHandler : IRequestHandler<PayOnlineCommand, BookingDto?>
	{
		private readonly BookingService _bookingService;

		public PayOnlineCommandHandler(BookingService bookingService)
		{
			_bookingService = bookingService;
		}

		public Task<BookingDto?> Handle(PayOnlineCommand request, CancellationToken cancellationToken)
		{
			return _bookingService.PayOnlineAsync(request.PassengerId, request.BookingId, request.Request);
		}
	}
}
