using MediatR;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.Rides;
using RideMateAPI.DTOs;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[Route("api/rides")]
	public class RideSearchController : ControllerBase
	{
		private readonly IMediator _mediator;

		public RideSearchController(IMediator mediator)
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

		[HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery] SearchRideRequest request)
		{
			var result = await _mediator.Send(new SearchRidesQuery(request, GetUserIdFromClaims()));
			return Ok(result);
		}
	}
}
