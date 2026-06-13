using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RideMateAPI.Application.Admin;

namespace RideMateAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("driver-requests")]
        public async Task<IActionResult> GetDriverRequests()
        {
            var result = await _mediator.Send(new GetDriverRequestsQuery());
            return Ok(result);
        }

        [HttpGet("verification-requests")]
        public async Task<IActionResult> GetVerificationRequests()
        {
            var result = await _mediator.Send(new GetDriverRequestsQuery());
            return Ok(result);
        }

        [HttpGet("vehicle-requests")]
        public async Task<IActionResult> GetVehicleRequests()
        {
            var result = await _mediator.Send(new GetVehicleVerificationRequestsQuery());
            return Ok(result);
        }

        [HttpPost("approve/{documentId}")]
        public async Task<IActionResult> ApproveDriverRequest(Guid documentId)
        {
            try
            {
                var result = await _mediator.Send(new ApproveDriverRequestCommand(documentId));
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reject/{documentId}")]
        public async Task<IActionResult> RejectDriverRequest(Guid documentId)
        {
            try
            {
                var result = await _mediator.Send(new RejectDriverRequestCommand(documentId));
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("vehicles/{vehicleId}/verify")]
        public async Task<IActionResult> VerifyVehicle(Guid vehicleId)
        {
            try
            {
                var result = await _mediator.Send(new VerifyVehicleCommand(vehicleId));
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
