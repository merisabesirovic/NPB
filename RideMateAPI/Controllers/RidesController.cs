using System;
using System.Linq;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.Rides;
using RideMateAPI.DTOs;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class RidesController : ControllerBase
	{
		private readonly IMediator _mediator;

		public RidesController(IMediator mediator)
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
		public async Task<IActionResult> CreateRide([FromBody] CreateRideDto dto)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			// Optionally ensure user is verified driver - check in DB

			try
			{
				var created = await _mediator.Send(new CreateRideCommand(uid.Value, dto));
				return CreatedAtAction(nameof(GetRideById), new { id = created.Id }, created);
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

		[HttpGet("{id}")]
		public async Task<IActionResult> GetRideById(Guid id)
		{
			var ride = await _mediator.Send(new GetRideByIdQuery(id));
			if (ride == null) return NotFound();
			return Ok(ride);
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> GetMyCreatedRides()
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var rides = await _mediator.Send(new GetMyCreatedRidesQuery(uid.Value));
				return Ok(rides);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> UpdateRide(Guid id, [FromBody] UpdateRideDto dto)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();

			try
			{
				var updated = await _mediator.Send(new UpdateRideCommand(uid.Value, id, dto));
				if (updated == null) return NotFound();
				return Ok(updated);
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> DeleteRide(Guid id)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var ok = await _mediator.Send(new CancelRideCommand(uid.Value, id));
				if (!ok) return NotFound();
				return NoContent();
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

		[HttpPut("{id}/status")]
		[Authorize]
		public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] DTOs.ChangeRideStatusRequest req)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();

			try
			{
				var updated = await _mediator.Send(new ChangeRideStatusCommand(uid.Value, id, req));
				if (updated == null) return NotFound();
				return Ok(updated);
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
	}
}
