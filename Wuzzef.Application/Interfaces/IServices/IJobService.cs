using Wuzzef.Application.DTOs.Request;
using Wuzzef.Application.DTOs.Response;

namespace Wuzzef.Application.Interfaces.IServices
{
    public interface IJobService
    {
        Task<ApiResponse<JobResponse>> CreateJobAsync(CreateJobRequest request);
        Task<ApiResponse<JobResponse>> GetJobByIdAsync(int id);
        Task<ApiResponse<IEnumerable<JobResponse>>> GetAllJobsAsync(bool? isActive = null);
        Task<ApiResponse<JobResponse>> UpdateJobAsync(int id, UpdateJobRequest request);
        Task<ApiResponse<string>> CloseJobAsync(int id);
    }
}
