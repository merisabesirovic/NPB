using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Disputes
{
	public record CreateDisputeCommand(Guid UserId, CreateDisputeRequest Request) : IRequest<DisputeDto>;
	public record ChangeDisputeStatusCommand(Guid AdminId, Guid DisputeId, ChangeDisputeStatusRequest Request) : IRequest<DisputeDto?>;

	public class CreateDisputeCommandHandler : IRequestHandler<CreateDisputeCommand, DisputeDto>
	{
		private readonly DisputeService _disputeService;

		public CreateDisputeCommandHandler(DisputeService disputeService)
		{
			_disputeService = disputeService;
		}

		public Task<DisputeDto> Handle(CreateDisputeCommand request, CancellationToken cancellationToken)
		{
			return _disputeService.CreateDisputeAsync(request.UserId, request.Request);
		}
	}

	public class ChangeDisputeStatusCommandHandler : IRequestHandler<ChangeDisputeStatusCommand, DisputeDto?>
	{
		private readonly DisputeService _disputeService;

		public ChangeDisputeStatusCommandHandler(DisputeService disputeService)
		{
			_disputeService = disputeService;
		}

		public Task<DisputeDto?> Handle(ChangeDisputeStatusCommand request, CancellationToken cancellationToken)
		{
			return _disputeService.ChangeStatusAsync(request.AdminId, request.DisputeId, request.Request);
		}
	}
}
