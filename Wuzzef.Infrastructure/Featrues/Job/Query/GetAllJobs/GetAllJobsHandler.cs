using MediatR;
using Microsoft.EntityFrameworkCore;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IRepositories;

namespace Wuzzef.Infrastructure.Featrues.Job.Query.GetAllJobs
{
    public class GetAllJobsHandler : IRequestHandler<GetAllJobsQuery, ApiResponse<IEnumerable<JobResponse>>>
    {
        private readonly IRepository<Domain.Entities.Job> _jobRepository;

        public GetAllJobsHandler(IRepository<Domain.Entities.Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<ApiResponse<IEnumerable<JobResponse>>> Handle(GetAllJobsQuery request, CancellationToken cancellationToken)
        {
            var jobs = await _jobRepository.GetAsync(
                expression: request.IsActive.HasValue ? j => j.IsActive == request.IsActive.Value : null,
                include: q => q.Include(j => j.Applications),
                cancellationToken: cancellationToken);

            var response = jobs.Select(MapToResponse).ToList();
            return ApiResponse<IEnumerable<JobResponse>>.SuccessResult(response);
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
