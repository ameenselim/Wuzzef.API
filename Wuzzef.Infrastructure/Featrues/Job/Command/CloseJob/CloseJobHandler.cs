using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IRepositories;
using Wuzzef.Domain.Enums;

namespace Wuzzef.Infrastructure.Featrues.Job.Command.CloseJob
{
    public class CloseJobHandler : IRequestHandler<CloseJobCommand, ApiResponse<string>>
    {
        private readonly IRepository<Domain.Entities.Job> _jobRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CloseJobHandler(IRepository<Domain.Entities.Job> jobRepository, IHttpContextAccessor httpContextAccessor)
        {
            _jobRepository = jobRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<string>> Handle(CloseJobCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<string>.FailureResult("User is not authenticated.");
            }

            var job = await _jobRepository.GetOneAsync(j => j.Id == request.Id, tracked: true, cancellationToken: cancellationToken);
            if (job == null)
            {
                return ApiResponse<string>.FailureResult("Job not found.");
            }

            // Ownership authorization rule
            if (job.UserId != currentUserId)
            {
                return ApiResponse<string>.FailureResult("You are not authorized to delete this job. Only the job creator can delete it.");
            }

            job.Status = JobStatus.Closed;
            await _jobRepository.CommitAsync(cancellationToken);

            return ApiResponse<string>.SuccessResult("Job closed successfully.", "Job closed successfully.");
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
