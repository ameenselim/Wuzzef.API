using MediatR;
using Microsoft.AspNetCore.Identity;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Infrastructure.Identity;

namespace Wuzzef.Infrastructure.Featrues.Auth.Command.Register
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, ApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                return ApiResponse<string>.FailureResult("Email is already registered.", new List<string> { "A user with this email address already exists." });
            }

            var existingUserByName = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserByName != null)
            {
                return ApiResponse<string>.FailureResult("Username is already taken.", new List<string> { "A user with this username already exists." });
            }

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<string>.FailureResult("User registration failed.", errors);
            }

            return ApiResponse<string>.SuccessResult("User registered successfully.", "Registration completed successfully.");
        }
    }
}
