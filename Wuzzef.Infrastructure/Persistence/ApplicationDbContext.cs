using Microsoft.EntityFrameworkCore;
using Wuzzef.Domain.Entities;
using Wuzzef.Infrastructure.Identity;

namespace Wuzzef.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationTypeConfigurations).Assembly);

        }
    }
}
