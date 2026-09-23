using MediatR;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Wuzzef.Application.DTOs.Response;

namespace Wuzzef.Infrastructure.Featrues.Application.Command.CancelApplication
{
    public class CancelApplicationCommand : IRequest<ApiResponse<string>>
    {
        public int Id { get; set; }
    }
}
