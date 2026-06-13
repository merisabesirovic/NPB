using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.Disputes;
using RideMateAPI.DTOs;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class DisputesController : ControllerBase
	{
		private readonly IMediator _mediator;

		public DisputesController(IMediator mediator)
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
		public async Task<IActionResult> Create([FromBody] CreateDisputeRequest req)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var d = await _mediator.Send(new CreateDisputeCommand(uid.Value, req));
				return CreatedAtAction(nameof(GetDisputeDetails), new { id = d.Id }, d);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> GetMy()
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			var list = await _mediator.Send(new GetMyDisputesQuery(uid.Value));
			return Ok(list);
		}

		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetDisputeDetails(Guid id)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var d = await _mediator.Send(new GetDisputeDetailsQuery(uid.Value, id));
				if (d == null) return NotFound();
				return Ok(d);
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
		}

		[HttpGet("admin/all")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AdminGetAll()
		{
			var list = await _mediator.Send(new AdminGetAllDisputesQuery());
			return Ok(list);
		}

		[HttpPost("{id}/status")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeDisputeStatusRequest req)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var d = await _mediator.Send(new ChangeDisputeStatusCommand(uid.Value, id, req));
				if (d == null) return NotFound();
				return Ok(d);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}
	}
}
