using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Wuzzef.Domain.Enums;

namespace Wuzzef.Domain.Entities
{
    public class JobApplication
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int JobId { get; set; }
        public Job Job { get; set; } = null!;
        public JobApplicationStatus JobApplicationStatus { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime StatusUpdatedAt { get; set; }
    }
}
