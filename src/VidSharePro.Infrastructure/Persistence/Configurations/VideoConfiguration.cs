// VidSharePro.Infrastructure/Persistence/Configurations/VideoConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VidSharePro.Domain.Entities;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Title).HasMaxLength(255).IsRequired();
        builder.Property(v => v.OriginalFileName).HasMaxLength(500).IsRequired();

        // Index for faster lookups by Owner
        builder.HasIndex(v => v.OwnerId);

        // Relationship
        builder.HasMany(v => v.Formats)
               .WithOne()
               .HasForeignKey(f => f.VideoId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}