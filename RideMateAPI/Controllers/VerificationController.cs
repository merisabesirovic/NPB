using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RideMateAPI.Application.Verification;
using RideMateAPI.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace RideMateAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class VerificationController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly IConfiguration _config;

		public VerificationController(IMediator mediator, IConfiguration config)
		{
			_mediator = mediator;
			_config = config;
		}

		// Upload identity document. Requires authentication; support fallback if middleware did not authenticate (manual token validation)
		[HttpPost("upload-identity")]
		public async Task<IActionResult> UploadIdentityDocument([FromForm] IdentityUploadRequest req)
		{
			var userId = ResolveAuthenticatedUserId();
			if (userId == null) return Unauthorized();

			try
			{
				var result = await _mediator.Send(new UploadIdentityDocumentCommand(userId.Value, req.File));
				return Ok(result);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(ex.Message);
			}
		}

		[HttpPost("upload-passenger-identity")]
		public async Task<IActionResult> UploadPassengerIdentityDocument([FromForm] IdentityUploadRequest req)
		{
			var userId = ResolveAuthenticatedUserId();
			if (userId == null) return Unauthorized();

			try
			{
				var result = await _mediator.Send(new UploadPassengerIdentityDocumentCommand(userId.Value, req.File));
				return Ok(result);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(ex.Message);
			}
		}

		[HttpGet("pending")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetPending()
		{
			var result = await _mediator.Send(new GetPendingVerificationDocumentsQuery());
			return Ok(result);
		}

		[HttpPost("{id}/approve")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Approve(Guid id)
		{
			var result = await _mediator.Send(new ApproveVerificationDocumentCommand(id));
			if (result == null) return NotFound();
			return Ok(result);
		}

		[HttpPost("{id}/reject")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Reject(Guid id)
		{
			var result = await _mediator.Send(new RejectVerificationDocumentCommand(id));
			if (result == null) return NotFound();
			return Ok(result);
		}

		private Guid? ResolveAuthenticatedUserId()
		{
			if (User?.Identity?.IsAuthenticated == true)
			{
				var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
						  ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
				if (Guid.TryParse(sub, out var id)) return id;
			}

			var auth = Request.Headers["Authorization"].FirstOrDefault();
			if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer "))
			{
				return null;
			}

			var token = auth.Substring("Bearer ".Length).Trim();
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "super_secret_development_key_change_this");
				var parameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidIssuer = _config["Jwt:Issuer"] ?? "RideMateApi",
					ValidateAudience = true,
					ValidAudience = _config["Jwt:Audience"] ?? "RideMateApiClients",
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateLifetime = true,
					RoleClaimType = System.Security.Claims.ClaimTypes.Role,
					NameClaimType = System.Security.Claims.ClaimTypes.Name
				};

				var principal = tokenHandler.ValidateToken(token, parameters, out _);
				var sub = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
						  ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
				return Guid.TryParse(sub, out var id) ? id : null;
			}
			catch
			{
				return null;
			}
		}
	}
}
