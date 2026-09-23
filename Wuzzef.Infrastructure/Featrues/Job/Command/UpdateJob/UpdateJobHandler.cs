using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IRepositories;

namespace Wuzzef.Infrastructure.Featrues.Job.Command.UpdateJob
{
    public class UpdateJobHandler : IRequestHandler<UpdateJobCommand, ApiResponse<JobResponse>>
    {
        private readonly IRepository<Domain.Entities.Job> _jobRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateJobHandler(IRepository<Domain.Entities.Job> jobRepository, IHttpContextAccessor httpContextAccessor)
        {
            _jobRepository = jobRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<JobResponse>> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<JobResponse>.FailureResult("User is not authenticated.");
            }

            var job = await _jobRepository.GetOneAsync(j => j.Id == request.Id, tracked: true, cancellationToken: cancellationToken);
            if (job == null)
            {
                return ApiResponse<JobResponse>.FailureResult("Job not found.");
            }

            // Ownership authorization rule
            if (job.UserId != currentUserId)
            {
                return ApiResponse<JobResponse>.FailureResult("You are not authorized to update this job. Only the job creator can update it.");
            }

            job.Title = request.Title;
            job.Description = request.Description;
            job.IsActive = request.IsActive;
            job.Status = request.Status;

            _jobRepository.Update(job);
            await _jobRepository.CommitAsync(cancellationToken);

            return ApiResponse<JobResponse>.SuccessResult(MapToResponse(job), "Job updated successfully.");
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private static JobResponse MapToResponse(Domain.Entities.Job job)
        {
            return new JobResponse
            {
                Id = job.Id,
                Title = job.Title ?? string.Empty,
                Description = job.Description ?? string.Empty,
                IsActive = job.IsActive,
                UserId = job.UserId,
                ApplicationsCount = job.Applications?.Count ?? 0
            };
        }
    }
}
