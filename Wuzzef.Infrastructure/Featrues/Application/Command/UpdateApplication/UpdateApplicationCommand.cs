using MediatR;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Domain.Enums;

namespace Wuzzef.Infrastructure.Featrues.Application.Command.UpdateApplication
{
    public class UpdateApplicationCommand : IRequest<ApiResponse<ApplicationResponse>>
    {
        public int Id { get; set; }
        public JobApplicationStatus Status { get; set; }
    }
}
