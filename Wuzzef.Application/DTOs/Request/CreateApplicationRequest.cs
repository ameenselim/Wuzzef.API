using System.ComponentModel.DataAnnotations;

namespace Wuzzef.Application.DTOs.Request
{
    public class CreateApplicationRequest
    {
        [Required(ErrorMessage = "Job ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "A valid Job ID is required.")]
        public int JobId { get; set; }
    }
}
