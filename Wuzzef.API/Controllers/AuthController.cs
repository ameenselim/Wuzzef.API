using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wuzzef.Application.DTOs.Request;
using Wuzzef.Infrastructure.Featrues.Auth.Command.Login;
using Wuzzef.Infrastructure.Featrues.Auth.Command.Register;

namespace Wuzzef.API.Controllers
{
    /// <summary>
    /// Handles user authentication - registration and login.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The registration data including full name, username, email, and password.</param>
        /// <returns>A success message upon registration.</returns>
        /// <response code="200">User registered successfully.</response>
        /// <response code="400">Invalid request data or registration failed.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new RegisterCommand
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword
            };

            var result = await _mediator.Send(command);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="request">The login credentials (email/username and password).</param>
        /// <returns>Authentication response with JWT token and user details.</returns>
        /// <response code="200">Login successful. Returns JWT token.</response>
        /// <response code="400">Invalid credentials or login failed.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await _mediator.Send(command);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
