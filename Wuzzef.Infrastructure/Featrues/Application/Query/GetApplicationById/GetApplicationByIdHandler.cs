using MediatR;
using Microsoft.EntityFrameworkCore;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IRepositories;
using Wuzzef.Domain.Entities;

namespace Wuzzef.Infrastructure.Featrues.Application.Query.GetApplicationById
{
    public class GetApplicationByIdHandler : IRequestHandler<GetApplicationByIdQuery, ApiResponse<ApplicationResponse>>
    {
        private readonly IRepository<JobApplication> _applicationRepository;

        public GetApplicationByIdHandler(IRepository<JobApplication> applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        public async Task<ApiResponse<ApplicationResponse>> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetOneAsync(
                expression: a => a.Id == request.Id,
                include: q => q.Include(a => a.Job),
                cancellationToken: cancellationToken);

            if (application == null)
            {
                return ApiResponse<ApplicationResponse>.FailureResult("Application not found.");
            }

            return ApiResponse<ApplicationResponse>.SuccessResult(MapToResponse(application));
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
