using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RideMateAPI.Application.Profile;
using RideMateAPI.DTOs;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class ProfileController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly ILogger<ProfileController> _logger;

		public ProfileController(IMediator mediator, ILogger<ProfileController> logger)
		{
			_mediator = mediator;
			_logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> GetProfile()
		{
			try
			{
				var identity = ResolveUserIdentity();
				if (identity.IsMissing)
				{
					return Unauthorized(new { error = "Missing sub or email claim" });
				}

				var profile = await _mediator.Send(new GetProfileQuery(identity.UserId, identity.Email));
				if (profile == null) return NotFound();

				return Ok(profile);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "GetProfile: unexpected error");
				return StatusCode(500, new { error = "Internal server error" });
			}
		}

		[HttpPut]
		public async Task<IActionResult> UpdateProfile([FromForm] ProfileUpdateRequest req)
		{
			try
			{
				var identity = ResolveUserIdentity();
				if (identity.IsMissing)
				{
					return Unauthorized(new { error = "Missing sub or email claim" });
				}

				var updated = await _mediator.Send(new UpdateProfileCommand(identity.UserId, identity.Email, req));
				if (!updated) return NotFound();

				return Ok(new { message = "Profile updated" });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { error = ex.Message });
			}
			catch (DbUpdateConcurrencyException ex)
			{
				_logger.LogWarning(ex, "UpdateProfile: concurrency conflict");
				return Conflict(new { error = "Profile was changed while you were editing. Refresh profile and try again." });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "UpdateProfile: unexpected error");
				return StatusCode(500, new { error = "Internal server error" });
			}
		}

		private (Guid? UserId, string? Email, bool IsMissing) ResolveUserIdentity()
		{
			var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
					  ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
					  ?? User.FindFirst("sub")?.Value;

			if (!string.IsNullOrWhiteSpace(sub) && Guid.TryParse(sub, out var userId))
			{
				return (userId, null, false);
			}

			var email = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value
						?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

			return string.IsNullOrWhiteSpace(email)
				? (null, null, true)
				: (null, email, false);
		}
	}
}
