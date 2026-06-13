using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Rides
{
	public record GetRideByIdQuery(Guid RideId) : IRequest<RideDto?>;
	public record GetMyCreatedRidesQuery(Guid DriverId) : IRequest<List<RideDto>>;
	public record SearchRidesQuery(SearchRideRequest Request, Guid? RequestingUserId) : IRequest<RideListResponse>;

	public class GetRideByIdQueryHandler : IRequestHandler<GetRideByIdQuery, RideDto?>
	{
		private readonly RideService _rideService;

		public GetRideByIdQueryHandler(RideService rideService)
		{
			_rideService = rideService;
		}

		public Task<RideDto?> Handle(GetRideByIdQuery request, CancellationToken cancellationToken)
		{
			return _rideService.GetByIdAsync(request.RideId);
		}
	}

	public class GetMyCreatedRidesQueryHandler : IRequestHandler<GetMyCreatedRidesQuery, List<RideDto>>
	{
		private readonly RideService _rideService;

		public GetMyCreatedRidesQueryHandler(RideService rideService)
		{
			_rideService = rideService;
		}

		public Task<List<RideDto>> Handle(GetMyCreatedRidesQuery request, CancellationToken cancellationToken)
		{
			return _rideService.GetMyCreatedRidesAsync(request.DriverId);
		}
	}

	public class SearchRidesQueryHandler : IRequestHandler<SearchRidesQuery, RideListResponse>
	{
		private readonly RideService _rideService;

		public SearchRidesQueryHandler(RideService rideService)
		{
			_rideService = rideService;
		}

		public Task<RideListResponse> Handle(SearchRidesQuery request, CancellationToken cancellationToken)
		{
			return _rideService.SearchRidesAsync(request.Request, request.RequestingUserId);
		}
	}
}
