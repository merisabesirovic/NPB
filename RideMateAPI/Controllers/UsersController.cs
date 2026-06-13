using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.Users;
using RideMateAPI.DTOs;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsersController : ControllerBase
	{
		private readonly IMediator _mediator;

		public UsersController(IMediator mediator)
		{
			_mediator = mediator;
		}

		private Guid? GetUserIdFromClaims()
		{
			var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
					  ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
			if (Guid.TryParse(sub, out var id)) return id;
			return null;
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> GetMyProfile()
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			var p = await _mediator.Send(new GetMyUserProfileQuery(uid.Value));
			return Ok(p);
		}

		[HttpGet("{userId}")]
		[Authorize]
		public async Task<IActionResult> GetUserProfile(Guid userId)
		{
			var p = await _mediator.Send(new GetPublicUserProfileQuery(userId));
			if (p == null) return NotFound();
			return Ok(p);
		}

		[HttpGet("admin/all")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AdminGetAll()
		{
			var list = await _mediator.Send(new AdminGetAllUsersQuery());
			return Ok(list);
		}

		[HttpPost("admin/create")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AdminCreate([FromBody] CreateUserAdminRequest req)
		{
			if (!req.IsAdmin) return BadRequest(new { error = "Admin may only create another admin via this endpoint" });
			var created = await _mediator.Send(new AdminCreateUserCommand(req));
			return CreatedAtAction(nameof(GetUserProfile), new { userId = created.Id }, created);
		}

		[HttpDelete("admin/{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AdminDelete(Guid id)
		{
			var ok = await _mediator.Send(new AdminDeleteUserCommand(id));
			if (!ok) return NotFound();
			return NoContent();
		}

		[HttpPost("admin/{id}/change-password")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AdminChangePassword(Guid id, [FromBody] AdminChangePasswordRequest req)
		{
			var ok = await _mediator.Send(new AdminChangePasswordCommand(id, req));
			if (!ok) return NotFound();
			return NoContent();
		}

		[HttpPost("me/change-password")]
		[Authorize]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			var ok = await _mediator.Send(new ChangeMyPasswordCommand(uid.Value, req));
			if (!ok) return BadRequest(new { error = "Current password incorrect" });

			// require re-login by invalidating current tokens isn't implemented; return success and client must re-login
			return Ok(new { requireRelogin = true });
		}
	}
}
