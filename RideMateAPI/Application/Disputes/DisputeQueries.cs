using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Disputes
{
	public record GetMyDisputesQuery(Guid UserId) : IRequest<List<DisputeDto>>;
	public record GetDisputeDetailsQuery(Guid UserId, Guid DisputeId) : IRequest<DisputeDto?>;
	public record AdminGetAllDisputesQuery() : IRequest<List<DisputeDto>>;

	public class GetMyDisputesQueryHandler : IRequestHandler<GetMyDisputesQuery, List<DisputeDto>>
	{
		private readonly DisputeService _disputeService;

		public GetMyDisputesQueryHandler(DisputeService disputeService)
		{
			_disputeService = disputeService;
		}

		public Task<List<DisputeDto>> Handle(GetMyDisputesQuery request, CancellationToken cancellationToken)
		{
			return _disputeService.GetMyDisputesAsync(request.UserId);
		}
	}

	public class GetDisputeDetailsQueryHandler : IRequestHandler<GetDisputeDetailsQuery, DisputeDto?>
	{
		private readonly DisputeService _disputeService;

		public GetDisputeDetailsQueryHandler(DisputeService disputeService)
		{
			_disputeService = disputeService;
		}

		public Task<DisputeDto?> Handle(GetDisputeDetailsQuery request, CancellationToken cancellationToken)
		{
			return _disputeService.GetDisputeDetailsAsync(request.UserId, request.DisputeId);
		}
	}

	public class AdminGetAllDisputesQueryHandler : IRequestHandler<AdminGetAllDisputesQuery, List<DisputeDto>>
	{
		private readonly DisputeService _disputeService;

		public AdminGetAllDisputesQueryHandler(DisputeService disputeService)
		{
			_disputeService = disputeService;
		}

		public Task<List<DisputeDto>> Handle(AdminGetAllDisputesQuery request, CancellationToken cancellationToken)
		{
			return _disputeService.AdminGetAllAsync();
		}
	}
}
