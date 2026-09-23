using MediatR;
using Wuzzef.Application.DTOs.Response;

namespace Wuzzef.Infrastructure.Featrues.Application.Query.GetApplicationById
{
    public class GetApplicationByIdQuery : IRequest<ApiResponse<ApplicationResponse>>
    {
        public int Id { get; set; }
    }
}
