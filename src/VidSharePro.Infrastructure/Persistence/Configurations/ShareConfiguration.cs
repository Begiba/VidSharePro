// VidSharePro.Infrastructure/Persistence/Configurations/ShareConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ShareConfiguration : IEntityTypeConfiguration<Share>
{
    public void Configure(EntityTypeBuilder<Share> builder)
    {
        builder.HasIndex(s => s.Token).IsUnique(); // Critical for performance
        builder.Property(s => s.Token).HasMaxLength(100).IsRequired();
    }
}