using Wuzzef.Application.DTOs.Request;
using Wuzzef.Application.DTOs.Response;

namespace Wuzzef.Application.Interfaces.IServices
{
    public interface IApplicationService
    {
        Task<ApiResponse<ApplicationResponse>> CreateApplicationAsync(CreateApplicationRequest request);
        Task<ApiResponse<ApplicationResponse>> GetApplicationByIdAsync(int id);
        Task<ApiResponse<IEnumerable<ApplicationResponse>>> GetAllApplicationsAsync(int? jobId = null);
        Task<ApiResponse<ApplicationResponse>> UpdateApplicationAsync(int applicationId, UpdateApplicationRequest request);
        Task<ApiResponse<string>> CancelApplicationAsync(int id);
    }
}
