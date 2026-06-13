using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.Bookings;
using RideMateAPI.DTOs;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class BookingsController : ControllerBase
	{
		private readonly IMediator _mediator;

		public BookingsController(IMediator mediator)
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

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] CreateBookingRequest req)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var b = await _mediator.Send(new CreateBookingCommand(uid.Value, req));
				return CreatedAtAction(nameof(GetMyBookings), new { id = b.Id }, b);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
			catch (Exception ex)
			{
				// Unexpected errors
				return StatusCode(500, new { error = ex.Message });
			}
		}

		[HttpPost("{id}/approve")]
		[Authorize]
		public async Task<IActionResult> Approve(Guid id)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var b = await _mediator.Send(new ApproveBookingCommand(uid.Value, id));
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

		[HttpPost("{id}/reject")]
		[Authorize]
		public async Task<IActionResult> Reject(Guid id)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var b = await _mediator.Send(new RejectBookingCommand(uid.Value, id));
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

		[HttpPost("{id}/cancel")]
		[Authorize]
		public async Task<IActionResult> Cancel(Guid id)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var b = await _mediator.Send(new CancelBookingCommand(uid.Value, id));
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

		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingRequest req)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var b = await _mediator.Send(new UpdateBookingCommand(uid.Value, id, req));
				if (b == null) return NotFound();
				return Ok(b);
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost("{id}/pay-online")]
		[Authorize]
		public async Task<IActionResult> PayOnline(Guid id, [FromBody] MockOnlinePaymentRequest req)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var b = await _mediator.Send(new PayOnlineCommand(uid.Value, id, req));
				if (b == null) return NotFound();
				return Ok(b);
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> GetMyBookings()
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			var list = await _mediator.Send(new GetMyBookingsQuery(uid.Value));
			return Ok(list);
		}

		[HttpGet("driver/requests")]
		[Authorize]
		public async Task<IActionResult> GetDriverBookingRequests()
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var list = await _mediator.Send(new GetDriverBookingsQuery(uid.Value));
				return Ok(list);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}
	}
}
