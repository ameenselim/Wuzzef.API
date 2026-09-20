using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wuzzef.Domain.Entities;
using Wuzzef.Infrastructure.Identity;

namespace Wuzzef.Infrastructure.Persistence.EntityTypeConfigurations
{
    public class JobTypeConfigurations : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
