using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;

namespace RideMateAPI.Application.Debug
{
	public record DecodeTokenQuery(string AuthorizationHeader) : IRequest<object>;
	public record GetClaimsQuery(ClaimsPrincipal User) : IRequest<object>;

	public class DecodeTokenQueryHandler : IRequestHandler<DecodeTokenQuery, object>
	{
		public Task<object> Handle(DecodeTokenQuery request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request.AuthorizationHeader))
			{
				throw new ArgumentException("Missing Authorization header");
			}

			var parts = request.AuthorizationHeader.Split(' ');
			if (parts.Length != 2 || !parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
			{
				throw new FormatException("Invalid Authorization header format. Use: Bearer <token>");
			}

			var handler = new JwtSecurityTokenHandler();
			var jwt = handler.ReadJwtToken(parts[1]);
			var claims = jwt.Claims.Select(c => new { c.Type, c.Value }).ToList();

			return Task.FromResult<object>(new
			{
				header = jwt.Header,
				payload = jwt.Payload,
				claims
			});
		}
	}

	public class GetClaimsQueryHandler : IRequestHandler<GetClaimsQuery, object>
	{
		public Task<object> Handle(GetClaimsQuery request, CancellationToken cancellationToken)
		{
			var claims = request.User.Claims.Select(c => new { c.Type, c.Value }).ToList();
			return Task.FromResult<object>(new
			{
				authenticated = request.User.Identity?.IsAuthenticated ?? false,
				claims
			});
		}
	}
}
