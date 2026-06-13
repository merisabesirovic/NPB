using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Rides
{
	public record CreateRideCommand(Guid DriverId, CreateRideDto Request) : IRequest<RideDto>;
	public record UpdateRideCommand(Guid DriverId, Guid RideId, UpdateRideDto Request) : IRequest<RideDto?>;
	public record CancelRideCommand(Guid DriverId, Guid RideId) : IRequest<bool>;
	public record ChangeRideStatusCommand(Guid DriverId, Guid RideId, ChangeRideStatusRequest Request) : IRequest<RideDto?>;

	public class CreateRideCommandHandler : IRequestHandler<CreateRideCommand, RideDto>
	{
		private readonly RideService _rideService;

		public CreateRideCommandHandler(RideService rideService)
		{
			_rideService = rideService;
		}

		public Task<RideDto> Handle(CreateRideCommand request, CancellationToken cancellationToken)
		{
			return _rideService.CreateRideAsync(request.DriverId, request.Request);
		}
	}

	public class UpdateRideCommandHandler : IRequestHandler<UpdateRideCommand, RideDto?>
	{
		private readonly RideService _rideService;

		public UpdateRideCommandHandler(RideService rideService)
		{
			_rideService = rideService;
		}

		public Task<RideDto?> Handle(UpdateRideCommand request, CancellationToken cancellationToken)
		{
			return _rideService.UpdateRideAsync(request.DriverId, request.RideId, request.Request);
		}
	}

	public class CancelRideCommandHandler : IRequestHandler<CancelRideCommand, bool>
	{
		private readonly RideService _rideService;

		public CancelRideCommandHandler(RideService rideService)
		{
			_rideService = rideService;
		}

		public Task<bool> Handle(CancelRideCommand request, CancellationToken cancellationToken)
		{
			return _rideService.CancelRideAsync(request.DriverId, request.RideId);
		}
	}

	public class ChangeRideStatusCommandHandler : IRequestHandler<ChangeRideStatusCommand, RideDto?>
	{
		private readonly RideService _rideService;

		public ChangeRideStatusCommandHandler(RideService rideService)
		{
			_rideService = rideService;
		}

		public Task<RideDto?> Handle(ChangeRideStatusCommand request, CancellationToken cancellationToken)
		{
			return _rideService.ChangeStatusAsync(request.DriverId, request.RideId, request.Request.Status);
		}
	}
}
