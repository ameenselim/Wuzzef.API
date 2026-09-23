using MediatR;
using Microsoft.EntityFrameworkCore;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IRepositories;

namespace Wuzzef.Infrastructure.Featrues.Job.Query.GetJobById
{
    public class GetJobByIdHandler : IRequestHandler<GetJobByIdQuery, ApiResponse<JobResponse>>
    {
        private readonly IRepository<Domain.Entities.Job> _jobRepository;

        public GetJobByIdHandler(IRepository<Domain.Entities.Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<ApiResponse<JobResponse>> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
        {
            var job = await _jobRepository.GetOneAsync(
                expression: j => j.Id == request.Id,
                include: q => q.Include(j => j.Applications),
                cancellationToken: cancellationToken);

            if (job == null)
            {
                return ApiResponse<JobResponse>.FailureResult("Job not found.");
            }

            return ApiResponse<JobResponse>.SuccessResult(MapToResponse(job));
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
