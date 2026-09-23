using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IRepositories;
using Wuzzef.Domain.Entities;
using Wuzzef.Domain.Enums;

namespace Wuzzef.Infrastructure.Featrues.Application.Command.CancelApplication
{
    public class CancelApplicationHandler : IRequestHandler<CancelApplicationCommand, ApiResponse<string>>
    {
        private readonly IRepository<JobApplication> _applicationRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CancelApplicationHandler(IRepository<JobApplication> applicationRepository ,IHttpContextAccessor httpContextAccessor)
        {
            _applicationRepository = applicationRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ApiResponse<string>> Handle(CancelApplicationCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ApiResponse<string>.FailureResult("User is not authenticated.");
            }

            var application = await _applicationRepository.GetOneAsync(
                expression: a => a.Id == request.Id,
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
    }
}
