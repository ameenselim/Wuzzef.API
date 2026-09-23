using MediatR;
using Wuzzef.Application.DTOs.Response;

namespace Wuzzef.Infrastructure.Featrues.Job.Query.GetAllJobs
{
    public class GetAllJobsQuery : IRequest<ApiResponse<IEnumerable<JobResponse>>>
    {
        public bool? IsActive { get; set; }
    }
}
