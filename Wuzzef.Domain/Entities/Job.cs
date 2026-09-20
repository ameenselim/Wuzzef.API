

using Wuzzef.Domain.Enums;

namespace Wuzzef.Domain.Entities
{
    public class Job 
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string UserId { get; set; } = string.Empty;

        public JobStatus Status { get; set; } = JobStatus.Open;

        public ICollection<JobApplication> Applications { get; set; } = new HashSet<JobApplication>();
    }
}
