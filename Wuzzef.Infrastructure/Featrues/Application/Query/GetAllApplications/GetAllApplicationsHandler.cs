using MediatR;
using Microsoft.EntityFrameworkCore;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IRepositories;
using Wuzzef.Domain.Entities;

namespace Wuzzef.Infrastructure.Featrues.Application.Query.GetAllApplications
{
    public class GetAllApplicationsHandler : IRequestHandler<GetAllApplicationsQuery, ApiResponse<IEnumerable<ApplicationResponse>>>
    {
        private readonly IRepository<JobApplication> _applicationRepository;

        public GetAllApplicationsHandler(IRepository<JobApplication> applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        public async Task<ApiResponse<IEnumerable<ApplicationResponse>>> Handle(GetAllApplicationsQuery request, CancellationToken cancellationToken)
        {
            var applications = await _applicationRepository.GetAsync(
                expression: request.JobId.HasValue ? a => a.JobId == request.JobId.Value : null,
                include: q => q.Include(a => a.Job),
                cancellationToken: cancellationToken);

            var response = applications.Select(MapToResponse).ToList();
            return ApiResponse<IEnumerable<ApplicationResponse>>.SuccessResult(response);
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
