using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideMateAPI.Application.Debug;

namespace RideMateAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DebugController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Development-only endpoint: decodes a JWT token without validating it so you can inspect claims.
        // Call with header: Authorization: Bearer <token>
        [HttpGet("decode-token")]
        [AllowAnonymous]
        public async Task<IActionResult> DecodeToken()
        {
            try
            {
                var result = await _mediator.Send(new DecodeTokenQuery(Request.Headers["Authorization"].ToString()));
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (FormatException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to decode token", detail = ex.Message });
            }
        }

        // Returns the claims as seen by the server for an authenticated request
        [HttpGet("claims")]
        [Authorize]
        public async Task<IActionResult> GetClaims()
        {
            var result = await _mediator.Send(new GetClaimsQuery(User));
            return Ok(result);
        }
    }
}
