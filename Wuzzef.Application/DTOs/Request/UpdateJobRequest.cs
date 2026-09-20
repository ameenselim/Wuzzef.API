using System.ComponentModel.DataAnnotations;
using Wuzzef.Domain.Enums;

namespace Wuzzef.Application.DTOs.Request
{
    public class UpdateJobRequest
    {
        [Required(ErrorMessage = "Job title is required.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Job title must be between 3 and 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job description is required.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Job description must be between 10 and 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        public JobStatus status { get; set; } = JobStatus.Open;
        public bool IsActive { get; set; }
    }
}
