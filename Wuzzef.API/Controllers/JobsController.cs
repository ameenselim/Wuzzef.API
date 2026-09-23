using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wuzzef.Application.DTOs.Request;
using Wuzzef.Infrastructure.Featrues.Job.Command.CloseJob;
using Wuzzef.Infrastructure.Featrues.Job.Command.CreateJob;
using Wuzzef.Infrastructure.Featrues.Job.Command.UpdateJob;
using Wuzzef.Infrastructure.Featrues.Job.Query.GetAllJobs;
using Wuzzef.Infrastructure.Featrues.Job.Query.GetJobById;

namespace Wuzzef.API.Controllers
{
    /// <summary>
    /// Manages job postings - create, update, retrieve, and close jobs.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all jobs, optionally filtered by active status.
        /// </summary>
        /// <param name="isActive">Optional filter: true for active jobs, false for inactive.</param>
        /// <returns>A list of jobs.</returns>
        /// <response code="200">Returns the list of jobs.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
        {
            var result = await _mediator.Send(new GetAllJobsQuery { IsActive = isActive });
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific job by its ID.
        /// </summary>
        /// <param name="id">The job ID.</param>
        /// <returns>The job details.</returns>
        /// <response code="200">Returns the job.</response>
        /// <response code="404">Job not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetJobByIdQuery { Id = id });
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Creates a new job posting. Requires authentication.
        /// </summary>
        /// <param name="request">The job creation data.</param>
        /// <returns>The created job.</returns>
        /// <response code="201">Job created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">User is not authenticated.</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateJobRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new CreateJobCommand
            {
                Title = request.Title,
                Description = request.Description,
                IsActive = request.IsActive
            };

            var result = await _mediator.Send(command);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        /// <summary>
        /// Updates an existing job. Only the job creator can update it. Requires authentication.
        /// </summary>
        /// <param name="id">The job ID to update.</param>
        /// <param name="request">The updated job data.</param>
        /// <returns>The updated job.</returns>
        /// <response code="200">Job updated successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized to update this job.</response>
        /// <response code="404">Job not found.</response>
        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateJobRequest request)
        {
            var command = new UpdateJobCommand
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                IsActive = request.IsActive,
                Status = request.status
            };

            var result = await _mediator.Send(command);
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

        /// <summary>
        /// Closes a job posting. Only the job creator can close it. Requires authentication.
        /// </summary>
        /// <param name="id">The job ID to close.</param>
        /// <returns>Confirmation message.</returns>
        /// <response code="200">Job closed successfully.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized to close this job.</response>
        /// <response code="404">Job not found.</response>
        [Authorize]
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Close(int id)
        {
            var result = await _mediator.Send(new CloseJobCommand { Id = id });
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
