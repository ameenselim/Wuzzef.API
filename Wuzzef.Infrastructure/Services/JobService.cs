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
    public class JobService : IJobService
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JobService(
            IRepository<Job> jobRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _jobRepository = jobRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<JobResponse>> CreateJobAsync(CreateJobRequest request)
        {
            if (request == null)
            {
                return ApiResponse<JobResponse>.FailureResult("Request cannot be null.");
            }

            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<JobResponse>.FailureResult("User is not authenticated.");
            }

            var job = new Job
            {
                Title = request.Title,
                Description = request.Description,
                IsActive = request.IsActive,
                UserId = currentUserId
            };

            await _jobRepository.CreateAsync(job);
            await _jobRepository.CommitAsync();

            return ApiResponse<JobResponse>.SuccessResult(MapToResponse(job), "Job created successfully.");
        }

        public async Task<ApiResponse<JobResponse>> GetJobByIdAsync(int id)
        {
            var job = await _jobRepository.GetOneAsync(
                expression: j => j.Id == id,
                include: q => q.Include(j => j.Applications));

            if (job == null)
            {
                return ApiResponse<JobResponse>.FailureResult("Job not found.");
            }

            return ApiResponse<JobResponse>.SuccessResult(MapToResponse(job));
        }

        public async Task<ApiResponse<IEnumerable<JobResponse>>> GetAllJobsAsync(bool? isActive = null)
        {
            var jobs = await _jobRepository.GetAsync(
                expression: isActive.HasValue ? j => j.IsActive == isActive.Value : null,
                include: q => q.Include(j => j.Applications));

            var response = jobs.Select(MapToResponse).ToList();
            return ApiResponse<IEnumerable<JobResponse>>.SuccessResult(response);
        }

        public async Task<ApiResponse<JobResponse>> UpdateJobAsync(int id, UpdateJobRequest request)
        {
            if (request == null)
            {
                return ApiResponse<JobResponse>.FailureResult("Request cannot be null.");
            }

            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<JobResponse>.FailureResult("User is not authenticated.");
            }

            var job = await _jobRepository.GetOneAsync(j => j.Id == id, tracked: true);
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
            job.Status = request.status;

            _jobRepository.Update(job);
            await _jobRepository.CommitAsync();

            return ApiResponse<JobResponse>.SuccessResult(MapToResponse(job), "Job updated successfully.");
        }

        public async Task<ApiResponse<string>> CloseJobAsync(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<string>.FailureResult("User is not authenticated.");
            }

            var job = await _jobRepository.GetOneAsync(j => j.Id == id, tracked: true);
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
            await _jobRepository.CommitAsync();

            return ApiResponse<string>.SuccessResult("Job closed successfully.", "Job closed successfully.");
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private static JobResponse MapToResponse(Job job)
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
