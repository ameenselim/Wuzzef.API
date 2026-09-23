using MediatR;
using Wuzzef.Application.DTOs.Response;

namespace Wuzzef.Infrastructure.Featrues.Application.Query.GetAllApplications
{
    public class GetAllApplicationsQuery : IRequest<ApiResponse<IEnumerable<ApplicationResponse>>>
    {
        public int? JobId { get; set; }
    }
}
