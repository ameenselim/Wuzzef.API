using MediatR;
using Wuzzef.Application.DTOs.Response;

namespace Wuzzef.Infrastructure.Featrues.Job.Query.GetJobById
{
    public class GetJobByIdQuery : IRequest<ApiResponse<JobResponse>>
    {
        public int Id { get; set; }
    }
}
