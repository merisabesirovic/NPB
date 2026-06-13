using RideMateAPI.DTOs;
using RideMateAPI.Models;

namespace RideMateAPI.Services
{
	public class AuthSessionService
	{
		private readonly TokenService _tokenService;

		public AuthSessionService(TokenService tokenService)
		{
			_tokenService = tokenService;
		}

		public AuthResponse Issue(User user, string ipAddress)
		{
			var effectiveRoles = UserRoleHelper.EffectiveRoles(user);
			var access = _tokenService.GenerateAccessToken(user, effectiveRoles);
			var refresh = _tokenService.GenerateRefreshToken();

			user.RefreshTokens.Add(new RefreshToken
			{
				UserId = user.Id,
				TokenHash = refresh.TokenHash,
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = refresh.ExpiresAt,
				CreatedByIp = ipAddress
			});

			return new AuthResponse
			{
				Token = access.Token,
				TokenExpiresAt = access.ExpiresAt,
				RefreshToken = refresh.Token,
				RefreshTokenExpiresAt = refresh.ExpiresAt,
				DriverVerificationPending = UserRoleHelper.HasPendingDriverVerification(user)
			};
		}

		public string HashRefreshToken(string refreshToken)
		{
			return _tokenService.HashRefreshToken(refreshToken);
		}
	}
}
