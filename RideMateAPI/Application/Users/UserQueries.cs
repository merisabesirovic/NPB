using MediatR;
using RideMateAPI.DTOs;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Users
{
	public record GetMyUserProfileQuery(Guid UserId) : IRequest<UserProfileDto?>;
	public record GetPublicUserProfileQuery(Guid UserId) : IRequest<UserProfileDto?>;
	public record AdminGetAllUsersQuery() : IRequest<List<UserProfileDto>>;

	public class GetMyUserProfileQueryHandler : IRequestHandler<GetMyUserProfileQuery, UserProfileDto?>
	{
		private readonly UserService _userService;

		public GetMyUserProfileQueryHandler(UserService userService)
		{
			_userService = userService;
		}

		public Task<UserProfileDto?> Handle(GetMyUserProfileQuery request, CancellationToken cancellationToken)
		{
			return _userService.GetProfileAsync(request.UserId);
		}
	}

	public class GetPublicUserProfileQueryHandler : IRequestHandler<GetPublicUserProfileQuery, UserProfileDto?>
	{
		private readonly UserService _userService;

		public GetPublicUserProfileQueryHandler(UserService userService)
		{
			_userService = userService;
		}

		public Task<UserProfileDto?> Handle(GetPublicUserProfileQuery request, CancellationToken cancellationToken)
		{
			return _userService.GetUserProfilePublicAsync(request.UserId);
		}
	}

	public class AdminGetAllUsersQueryHandler : IRequestHandler<AdminGetAllUsersQuery, List<UserProfileDto>>
	{
		private readonly UserService _userService;

		public AdminGetAllUsersQueryHandler(UserService userService)
		{
			_userService = userService;
		}

		public Task<List<UserProfileDto>> Handle(AdminGetAllUsersQuery request, CancellationToken cancellationToken)
		{
			return _userService.AdminGetAllAsync();
		}
	}
}
