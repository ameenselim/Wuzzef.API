using MediatR;
using Wuzzef.Application.DTOs.Response;

namespace Wuzzef.Infrastructure.Featrues.Job.Command.CloseJob
{
    public class CloseJobCommand : IRequest<ApiResponse<string>>
    {
        public int Id { get; set; }
    }
}
