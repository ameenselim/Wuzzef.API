using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wuzzef.Domain.Entities;
using Wuzzef.Infrastructure.Identity;
    public class ApplicationTypeConfigurations : IEntityTypeConfiguration<JobApplication>
    {
        public void Configure(EntityTypeBuilder<JobApplication> builder)
        {
            builder.HasIndex(x => new { x.UserId, x.JobId })
                .IsUnique();

            builder.HasOne<ApplicationUser>()
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Job)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }