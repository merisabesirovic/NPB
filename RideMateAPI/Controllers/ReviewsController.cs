using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.Reviews;
using RideMateAPI.DTOs;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ReviewsController : ControllerBase
	{
		private readonly IMediator _mediator;

		public ReviewsController(IMediator mediator)
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
		public async Task<IActionResult> Create([FromBody] CreateReviewRequest req)
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			try
			{
				var r = await _mediator.Send(new CreateReviewCommand(uid.Value, req));
				return CreatedAtAction(nameof(GetUserReviews), new { userId = r.ReviewedUserId }, r);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
			catch (UnauthorizedAccessException ex)
			{
				return Forbid();
			}
		}

		[HttpGet("user/{userId}")]
		public async Task<IActionResult> GetUserReviews(Guid userId)
		{
			var list = await _mediator.Send(new GetUserReviewsQuery(userId));
			return Ok(list);
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> GetMyReviews()
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			var list = await _mediator.Send(new GetMyReviewsQuery(uid.Value));
			return Ok(list);
		}

		[HttpGet("me/written")]
		[Authorize]
		public async Task<IActionResult> GetMyWrittenReviews()
		{
			var uid = GetUserIdFromClaims();
			if (uid == null) return Unauthorized();
			var list = await _mediator.Send(new GetMyWrittenReviewsQuery(uid.Value));
			return Ok(list);
		}
	}
}
