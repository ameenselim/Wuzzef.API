using Wuzzef.Application.DTOs.Request;
using Wuzzef.Application.DTOs.Response;

namespace Wuzzef.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> RegisterAsync(RegisterRequest request);
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    }
}
