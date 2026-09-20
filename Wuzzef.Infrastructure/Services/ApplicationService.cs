using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wuzzef.Application.DTOs.Request;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IRepositories;
using Wuzzef.Application.Interfaces.IServices;
using Wuzzef.Domain.Entities;
using Wuzzef.Domain.Enums;

namespace Wuzzef.Infrastructure.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IRepository<JobApplication> _applicationRepository;
        private readonly IRepository<Job> _jobRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationService(
            IRepository<JobApplication> applicationRepository,
            IRepository<Job> jobRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<ApplicationResponse>> CreateApplicationAsync(CreateApplicationRequest request)
        {
            if (request == null)
            {
                return ApiResponse<ApplicationResponse>.FailureResult("Request cannot be null.");
            }

            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<ApplicationResponse>.FailureResult("User is not authenticated.");
            }

            var job = await _jobRepository.GetOneAsync(j => j.Id == request.JobId);
            if (job == null)
            {
                return ApiResponse<ApplicationResponse>.FailureResult("Job not found.");
            }

            if (!job.IsActive)
            {
                return ApiResponse<ApplicationResponse>.FailureResult("Cannot apply to an inactive job.");
            }

            var existingApplication = await _applicationRepository.GetOneAsync(
                a => a.UserId == currentUserId && a.JobId == request.JobId);
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

            await _applicationRepository.CreateAsync(application);
            await _applicationRepository.CommitAsync();

            return ApiResponse<ApplicationResponse>.SuccessResult(MapToResponse(application), "Application submitted successfully.");
        }

        public async Task<ApiResponse<ApplicationResponse>> GetApplicationByIdAsync(int id)
        {
            var application = await _applicationRepository.GetOneAsync(
                expression: a => a.Id == id,
                include: q => q.Include(a => a.Job));

            if (application == null)
            {
                return ApiResponse<ApplicationResponse>.FailureResult("Application not found.");
            }

            return ApiResponse<ApplicationResponse>.SuccessResult(MapToResponse(application));
        }

        public async Task<ApiResponse<IEnumerable<ApplicationResponse>>> GetAllApplicationsAsync(int? jobId = null)
        {
            var applications = await _applicationRepository.GetAsync(
                expression: jobId.HasValue ? a => a.JobId == jobId.Value : null,
                include: q => q.Include(a => a.Job));

            var response = applications.Select(MapToResponse).ToList();
            return ApiResponse<IEnumerable<ApplicationResponse>>.SuccessResult(response);
        }

        public async Task<ApiResponse<ApplicationResponse>> UpdateApplicationAsync(int applicationId, UpdateApplicationRequest request)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<ApplicationResponse>.FailureResult("User is not authenticated.");
            }
            if (request == null)
            {
                return ApiResponse<ApplicationResponse>.FailureResult("Request cannot be null.");
            }

            var application = await _applicationRepository.GetOneAsync(
                expression: a => a.Id == applicationId,
                include: q => q.Include(a => a.Job),
                tracked: true);

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
            await _applicationRepository.CommitAsync();

            return ApiResponse<ApplicationResponse>.SuccessResult(MapToResponse(application), "Application status updated successfully.");
        }
        public async Task<ApiResponse<string>> CancelApplicationAsync(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<string>.FailureResult("User is not authenticated.");
            }

            var application = await _applicationRepository.GetOneAsync(
                expression: a => a.Id == id,
                tracked: true);

            if (application == null)
            {
                return ApiResponse<string>.FailureResult("Application not found.");
            }

            // التحقق من أن المستخدم الحالي هو صاحبه الطلب (Applicant)
            if (application.UserId != currentUserId)
            {
                return ApiResponse<string>.FailureResult("You are not authorized to cancel this application.");
            }

            // التحقق من أن حالة الطلب تسمح بالإلغاء (Applied أو UnderReview فقط)
            if (application.JobApplicationStatus != JobApplicationStatus.Applied &&
                application.JobApplicationStatus != JobApplicationStatus.UnderReview)
            {
                return ApiResponse<string>.FailureResult("You cannot cancel this application at its current status.");
            }

            _applicationRepository.Delete(application);
            await _applicationRepository.CommitAsync();

            return ApiResponse<string>.SuccessResult("Application cancelled successfully.", "Application cancelled successfully.");
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
