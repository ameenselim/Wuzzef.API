using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IRepositories;
using Wuzzef.Domain.Entities;

namespace Wuzzef.Infrastructure.Featrues.Application.Command.UpdateApplication
{
    public class UpdateApplicationHandler : IRequestHandler<UpdateApplicationCommand, ApiResponse<ApplicationResponse>>
    {
        private readonly IRepository<JobApplication> _applicationRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateApplicationHandler(
            IRepository<JobApplication> applicationRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _applicationRepository = applicationRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<ApplicationResponse>> Handle(UpdateApplicationCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<ApplicationResponse>.FailureResult("User is not authenticated.");
            }

            var application = await _applicationRepository.GetOneAsync(
                expression: a => a.Id == request.Id,
                include: q => q.Include(a => a.Job),
                tracked: true,
                cancellationToken: cancellationToken);

            if (application == null)
            {
                return ApiResponse<ApplicationResponse>.FailureResult("Application not found.");
            }

            // التحقق من أن المستخدم الحالي هو ناشر الوظيفة (Job.UserId)
            if (application.Job == null || application.Job.UserId != currentUserId)
            {
                return ApiResponse<ApplicationResponse>.FailureResult("You are not authorized to update this application status.");
            }

            application.JobApplicationStatus = request.Status;
            application.StatusUpdatedAt = DateTime.UtcNow;

            _applicationRepository.Update(application);
            await _applicationRepository.CommitAsync(cancellationToken);

            return ApiResponse<ApplicationResponse>.SuccessResult(MapToResponse(application), "Application status updated successfully.");
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
