using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Users
{
	public record AdminCreateUserCommand(CreateUserAdminRequest Request) : IRequest<UserProfileDto>;
	public record AdminDeleteUserCommand(Guid UserId) : IRequest<bool>;
	public record AdminChangePasswordCommand(Guid UserId, AdminChangePasswordRequest Request) : IRequest<bool>;
	public record ChangeMyPasswordCommand(Guid UserId, ChangePasswordRequest Request) : IRequest<bool>;

	public class AdminCreateUserCommandHandler : IRequestHandler<AdminCreateUserCommand, UserProfileDto>
	{
		private readonly UserService _userService;

		public AdminCreateUserCommandHandler(UserService userService)
		{
			_userService = userService;
		}

		public Task<UserProfileDto> Handle(AdminCreateUserCommand request, CancellationToken cancellationToken)
		{
			return _userService.AdminCreateUserAsync(request.Request);
		}
	}

	public class AdminDeleteUserCommandHandler : IRequestHandler<AdminDeleteUserCommand, bool>
	{
		private readonly UserService _userService;

		public AdminDeleteUserCommandHandler(UserService userService)
		{
			_userService = userService;
		}

		public Task<bool> Handle(AdminDeleteUserCommand request, CancellationToken cancellationToken)
		{
			return _userService.AdminDeleteUserAsync(request.UserId);
		}
	}

	public class AdminChangePasswordCommandHandler : IRequestHandler<AdminChangePasswordCommand, bool>
	{
		private readonly UserService _userService;

		public AdminChangePasswordCommandHandler(UserService userService)
		{
			_userService = userService;
		}

		public Task<bool> Handle(AdminChangePasswordCommand request, CancellationToken cancellationToken)
		{
			return _userService.AdminChangePasswordAsync(request.UserId, request.Request.NewPassword);
		}
	}

	public class ChangeMyPasswordCommandHandler : IRequestHandler<ChangeMyPasswordCommand, bool>
	{
		private readonly UserService _userService;

		public ChangeMyPasswordCommandHandler(UserService userService)
		{
			_userService = userService;
		}

		public Task<bool> Handle(ChangeMyPasswordCommand request, CancellationToken cancellationToken)
		{
			return _userService.ChangePasswordAsync(request.UserId, request.Request.CurrentPassword, request.Request.NewPassword);
		}
	}
}
