using MediatR;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Notifications
{
	public record MarkNotificationAsReadCommand(Guid UserId, Guid NotificationId) : IRequest<bool>;
	public record MarkAllNotificationsAsReadCommand(Guid UserId) : IRequest<int>;

	public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
	{
		private readonly NotificationService _notificationService;

		public MarkNotificationAsReadCommandHandler(NotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
		{
			return _notificationService.MarkAsReadAsync(request.UserId, request.NotificationId);
		}
	}

	public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, int>
	{
		private readonly NotificationService _notificationService;

		public MarkAllNotificationsAsReadCommandHandler(NotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public Task<int> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
		{
			return _notificationService.MarkAllAsReadAsync(request.UserId);
		}
	}
}
