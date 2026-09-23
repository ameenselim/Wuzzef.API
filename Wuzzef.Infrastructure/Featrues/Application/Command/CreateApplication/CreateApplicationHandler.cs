using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IRepositories;
using Wuzzef.Domain.Entities;
using Wuzzef.Domain.Enums;

namespace Wuzzef.Infrastructure.Featrues.Application.Command.CreateApplication
{
    public class CreateApplicationHandler : IRequestHandler<CreateApplicationCommand, ApiResponse<ApplicationResponse>>
    {
        private readonly IRepository<JobApplication> _applicationRepository;
        private readonly IRepository<Wuzzef.Domain.Entities.Job> _jobRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateApplicationHandler(
            IRepository<JobApplication> applicationRepository,
            IRepository<Wuzzef.Domain.Entities.Job> jobRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<ApplicationResponse>> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<ApplicationResponse>.FailureResult("User is not authenticated.");
            }

            var job = await _jobRepository.GetOneAsync(j => j.Id == request.JobId, cancellationToken: cancellationToken);
            if (job == null)
            {
                return ApiResponse<ApplicationResponse>.FailureResult("Job not found.");
            }

            if (!job.IsActive)
            {
                return ApiResponse<ApplicationResponse>.FailureResult("Cannot apply to an inactive job.");
            }

            var existingApplication = await _applicationRepository.GetOneAsync(
                a => a.UserId == currentUserId && a.JobId == request.JobId, cancellationToken: cancellationToken);
            if (existingApplication != null)
            {
                return ApiResponse<ApplicationResponse>.FailureResult("You have already applied for this job.");
            }

            var application = new JobApplication
            {
                UserId = currentUserId,
                JobId = request.JobId,
                Job = job,
                JobApplicationStatus = JobApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow,
                StatusUpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.CreateAsync(application, cancellationToken);
            await _applicationRepository.CommitAsync(cancellationToken);

            return ApiResponse<ApplicationResponse>.SuccessResult(MapToResponse(application), "Application submitted successfully.");
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private static ApplicationResponse MapToResponse(JobApplication application)
        {
            return new ApplicationResponse
            {
                Id = application.Id,
                UserId = application.UserId,
                JobId = application.JobId,
                JobTitle = application.Job?.Title ?? string.Empty,
                Status = application.JobApplicationStatus.ToString(),
                AppliedAt = application.AppliedAt,
                StatusUpdatedAt = application.StatusUpdatedAt
            };
        }
    }
}
