// VidSharePro.IntegrationTests/Repositories/VideoRepositoryTests.cs
using System;
using System.Threading.Tasks;
using VidSharePro.Domain.Entities;
using VidSharePro.Infrastructure.Persistence;
using VidSharePro.Infrastructure.Persistence.Repositories;
using VidSharePro.IntegrationTests;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public class VideoRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly AppDbContext _context;

    public VideoRepositoryTests(DatabaseFixture fixture) => _context = fixture.Context;

    [Fact]
    public async Task AddAsync_ShouldPersistVideo_AndApplySoftDeleteFilter()
    {
        var repo = new VideoRepository(_context);
        var video = new Video("Persist", "p.mp4", 500, "path/to/vid", Guid.NewGuid());

        await repo.AddAsync(video);
        await repo.DeleteAsync(video.Id); // Triggers Soft Delete

        var result = await _context.Videos.FindAsync(video.Id);
        result.Should().NotBeNull();
        result.IsDeleted.Should().BeTrue();

        // Global filter check
        var filteredResult = await _context.Videos.FirstOrDefaultAsync(v => v.Id == video.Id);
        filteredResult.Should().BeNull();
    }
}