using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VidSharePro.Application.Common.Configuration;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Application.Services;
using VidSharePro.Domain.Entities;


public class VideoServiceTests
{
    private readonly Mock<IVideoRepository> _repoMock;
    private readonly Mock<IUserRepository> _repoUserMock;
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly Mock<IJobRepository> _repoJobMock;
    private readonly Mock<ILogger<VideoService>> _loggerMock;
    private readonly Mock<IMetricsService> _metricsMock;
    private readonly Mock<IOptions<StorageOptions>> _storageOptionsMock;
    private readonly VideoService _service;

    public VideoServiceTests()
    {
        _repoMock = new Mock<IVideoRepository>();
        _repoUserMock = new Mock<IUserRepository>();
        _fileStorageMock = new Mock<IFileStorage>();
        _repoJobMock = new Mock<IJobRepository>();
        _loggerMock = new Mock<ILogger<VideoService>>();
        _metricsMock = new Mock<IMetricsService>();
        _storageOptionsMock = new Mock<IOptions<StorageOptions>>();

        _service = new VideoService(
            _repoMock.Object,
            _repoUserMock.Object,
            _fileStorageMock.Object,
            _repoJobMock.Object,
            _loggerMock.Object,
            _metricsMock.Object,
            _storageOptionsMock.Object
        );
    }

    [Fact]
    public async Task GetUserVideos_ShouldInvokeRepositoryCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ct = CancellationToken.None;

        // Match the signature: (Guid, bool, CancellationToken)
        _repoMock.Setup(repo => repo.GetByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<bool>(), // This matches the 'trackChanges' or 'includeFormats' bool
            It.IsAny<CancellationToken>()));

        // Act
        await _service.GetUserVideosAsync(userId, ct);

        // Assert
        // Change this line to match the Performed Invocations in the error log:
        _repoMock.Verify(x => x.GetByOwnerIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}