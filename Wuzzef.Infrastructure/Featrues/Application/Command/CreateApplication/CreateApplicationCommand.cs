using MediatR;
using Wuzzef.Application.DTOs.Response;

namespace Wuzzef.Infrastructure.Featrues.Application.Command.CreateApplication
{
    public class CreateApplicationCommand : IRequest<ApiResponse<ApplicationResponse>>
    {
        public int JobId { get; set; }
    }
}
