using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.Notifications;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class NotificationsController : ControllerBase
	{
		private readonly IMediator _mediator;

		public NotificationsController(IMediator mediator)
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
		public async Task<IActionResult> GetMy()
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			var list = await _mediator.Send(new GetMyNotificationsQuery(uid.Value));
			return Ok(list);
		}

		[HttpPost("{id}/read")]
		[Authorize]
		public async Task<IActionResult> MarkAsRead(Guid id)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			var ok = await _mediator.Send(new MarkNotificationAsReadCommand(uid.Value, id));
			if (!ok) return NotFound();
			return NoContent();
		}

		[HttpPost("read-all")]
		[Authorize]
		public async Task<IActionResult> MarkAllAsRead()
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			var count = await _mediator.Send(new MarkAllNotificationsAsReadCommand(uid.Value));
			return Ok(new { marked = count });
		}
	}
}
