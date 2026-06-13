using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.Auth;
using RideMateAPI.DTOs;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IMediator _mediator;

		public AuthController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("register")]
		[ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Register([FromForm] RegisterRequest req)
		{
			var created = await _mediator.Send(new RegisterCommand(req));
			return CreatedAtAction(nameof(Register), new { id = created.Id }, created);
		}

		[HttpPost("login")]
		[ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Login([FromBody] LoginRequest req)
		{
			var response = await _mediator.Send(new LoginCommand(req, IpAddress()));
			if (response == null)
			{
				return UnauthorizedProblem("Invalid credentials");
			}

			return Ok(response);
		}

		[HttpPost("refresh")]
		[ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
		{
			var response = await _mediator.Send(new RefreshTokenCommand(req, IpAddress()));
			if (response == null)
			{
				return UnauthorizedProblem("Invalid refresh token");
			}

			return Ok(response);
		}

		[HttpPost("revoke")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> Revoke([FromBody] RevokeRefreshTokenRequest req)
		{
			await _mediator.Send(new RevokeRefreshTokenCommand(req, IpAddress()));
			return NoContent();
		}

		[HttpGet("me")]
		[Authorize]
		[ProducesResponseType(typeof(AuthUserResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Me()
		{
			var userId = GetUserIdFromClaims();
			if (userId == null) return Unauthorized();

			var currentUser = await _mediator.Send(new GetCurrentUserQuery(userId.Value));
			return currentUser == null ? NotFound() : Ok(currentUser);
		}

		private Guid? GetUserIdFromClaims()
		{
			var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
					  ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
			return Guid.TryParse(sub, out var id) ? id : null;
		}

		private string IpAddress()
		{
			return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
		}

		private IActionResult UnauthorizedProblem(string title)
		{
			var problemDetails = new ProblemDetails
			{
				Status = StatusCodes.Status401Unauthorized,
				Title = title,
				Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
				Instance = HttpContext.Request.Path
			};
			problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;
			return Unauthorized(problemDetails);
		}
	}
}
