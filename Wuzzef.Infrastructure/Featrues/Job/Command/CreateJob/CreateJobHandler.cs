using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IRepositories;
using Wuzzef.Domain.Entities;

namespace Wuzzef.Infrastructure.Featrues.Job.Command.CreateJob
{
    public class CreateJobHandler : IRequestHandler<CreateJobCommand, ApiResponse<JobResponse>>
    {
        private readonly IRepository<Domain.Entities.Job> _jobRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateJobHandler(IRepository<Domain.Entities.Job> jobRepository, IHttpContextAccessor httpContextAccessor)
        {
            _jobRepository = jobRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<JobResponse>> Handle(CreateJobCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<JobResponse>.FailureResult("User is not authenticated.");
            }

            var job = new Domain.Entities.Job
            {
                Title = request.Title,
                Description = request.Description,
                IsActive = request.IsActive,
                UserId = currentUserId
            };

            await _jobRepository.CreateAsync(job, cancellationToken);
            await _jobRepository.CommitAsync(cancellationToken);

            return ApiResponse<JobResponse>.SuccessResult(MapToResponse(job), "Job created successfully.");
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
