using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.DriverPayments;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[Route("api/driver/payments")]
	public class DriverPaymentsController : ControllerBase
	{
		private readonly IMediator _mediator;

		public DriverPaymentsController(IMediator mediator)
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

		// Driver marks that passenger paid in cash when boarding
		[HttpPost("{bookingId}/mark-cash-paid")]
		[Authorize]
		public async Task<IActionResult> MarkCashPaid(Guid bookingId)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var b = await _mediator.Send(new MarkCashPaidCommand(uid.Value, bookingId));
				if (b == null) return NotFound();
				return Ok(b);
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}
	}
}
