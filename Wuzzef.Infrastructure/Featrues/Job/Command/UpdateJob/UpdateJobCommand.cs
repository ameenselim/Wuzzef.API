using MediatR;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Domain.Enums;

namespace Wuzzef.Infrastructure.Featrues.Job.Command.UpdateJob
{
    public class UpdateJobCommand : IRequest<ApiResponse<JobResponse>>
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public JobStatus Status { get; set; } = JobStatus.Open;
    }
}
