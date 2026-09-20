using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using Wuzzef.Domain.Entities;

namespace Wuzzef.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? CvUrl { get; set; }

        public ICollection<JobApplication> Applications { get; set; } = new HashSet<JobApplication>();
    }
}
