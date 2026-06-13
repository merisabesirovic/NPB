using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Notifications
{
	public record GetMyNotificationsQuery(Guid UserId) : IRequest<List<NotificationDto>>;

	public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, List<NotificationDto>>
	{
		private readonly NotificationService _notificationService;

		public GetMyNotificationsQueryHandler(NotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public Task<List<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
		{
			return _notificationService.GetMyNotificationsAsync(request.UserId);
		}
	}
}
