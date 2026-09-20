using System.ComponentModel.DataAnnotations;
using Wuzzef.Domain.Enums;

namespace Wuzzef.Application.DTOs.Request
{
    public class UpdateApplicationRequest
    {
        [Required(ErrorMessage = "Application status is required.")]
        public JobApplicationStatus Status { get; set; }
    }
}
