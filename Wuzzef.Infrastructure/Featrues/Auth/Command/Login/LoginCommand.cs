using MediatR;
using Wuzzef.Application.DTOs.Response;

namespace Wuzzef.Infrastructure.Featrues.Auth.Command.Login
{
    public class LoginCommand : IRequest<ApiResponse<AuthResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
