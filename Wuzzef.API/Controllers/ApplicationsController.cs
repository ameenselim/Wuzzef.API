using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wuzzef.Application.DTOs.Request;
using Wuzzef.Infrastructure.Featrues.Application.Command.CancelApplication;
using Wuzzef.Infrastructure.Featrues.Application.Command.CreateApplication;
using Wuzzef.Infrastructure.Featrues.Application.Command.UpdateApplication;
using Wuzzef.Infrastructure.Featrues.Application.Query.GetAllApplications;
using Wuzzef.Infrastructure.Featrues.Application.Query.GetApplicationById;

namespace Wuzzef.API.Controllers
{
    /// <summary>
    /// Manages job applications - create, update, retrieve, and cancel applications.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ApplicationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all applications, optionally filtered by job ID.
        /// </summary>
        /// <param name="jobId">Optional filter by job ID.</param>
        /// <returns>A list of applications.</returns>
        /// <response code="200">Returns the list of applications.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] int? jobId)
        {
            var result = await _mediator.Send(new GetAllApplicationsQuery { JobId = jobId });
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific application by its ID.
        /// </summary>
        /// <param name="id">The application ID.</param>
        /// <returns>The application details.</returns>
        /// <response code="200">Returns the application.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">Application not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetApplicationByIdQuery { Id = id });
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Submits a new job application. Requires authentication.
        /// </summary>
        /// <param name="request">The application creation data containing the Job ID.</param>
        /// <returns>The created application.</returns>
        /// <response code="201">Application submitted successfully.</response>
        /// <response code="400">Invalid request or duplicate application.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">Job not found.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateApplicationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new CreateApplicationCommand
            {
                JobId = request.JobId
            };

            var result = await _mediator.Send(command);
            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        /// <summary>
        /// Updates an application's status. Only the job poster can update application status. Requires authentication.
        /// </summary>
        /// <param name="id">The application ID to update.</param>
        /// <param name="request">The updated application status.</param>
        /// <returns>The updated application.</returns>
        /// <response code="200">Application status updated successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">Application not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateApplicationRequest request)
        {
            var command = new UpdateApplicationCommand
            {
                Id = id,
                Status = request.Status
            };

            var result = await _mediator.Send(command);
            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Cancels a job application. Only the applicant can cancel, and only if the status is Applied or UnderReview. Requires authentication.
        /// </summary>
        /// <param name="id">The application ID to cancel.</param>
        /// <returns>Confirmation message.</returns>
        /// <response code="200">Application cancelled successfully.</response>
        /// <response code="400">Cannot cancel at current status.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized to cancel this application.</response>
        /// <response code="404">Application not found.</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _mediator.Send(new CancelApplicationCommand { Id = id });
            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result);
                }
                if (result.Message.Contains("authorized", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
