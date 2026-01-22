using FluentAssertions;
using VidSharePro.Domain.Entities;
using VidSharePro.Domain.Enums;
using Xunit;

public class VideoTests
{
    [Fact]
    public void CompleteUpload_ShouldChangeStatusToPendingValidation()
    {
        // Arrange: Video starts in 'Uploading' by default (from our fix earlier)
        var video = new Video("Test Video", "video.mp4", 1024, "path/to/vid", Guid.NewGuid());

        // Act
        video.CompleteUpload();

        // Assert
        video.Status.Should().Be(VideoStatus.PendingValidation);
    }

    [Fact]
    public void MarkAsFailed_ShouldStoreReasonAndSetStatus()
    {
        // Arrange
        var video = new Video("Fail Test", "v.mp4", 500, "path", Guid.NewGuid());
        string reason = "Codec not supported";

        // Act
        video.MarkAsFailed(reason);

        // Assert
        video.Status.Should().Be(VideoStatus.Failed);
        video.FailureReason.Should().Be(reason);
    }

    [Fact]
    public void CompleteUpload_WhenNotUploading_ShouldThrowException()
    {
        // Arrange
        var video = new Video("Error Test", "v.mp4", 500, "path", Guid.NewGuid());
        video.MarkAsFailed("Already failed");

        // Act
        Action act = () => video.CompleteUpload();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*only be completed from the Uploading state*");
    }
    [Fact]
    public void StartProcessing_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var video = new Video("Test", "file.mp4", 100, "path", Guid.NewGuid());
        video.CompleteUpload(); // Move from Uploading to PendingValidation

        // Act
        video.StartProcessing();

        // Assert
        Assert.Equal(VideoStatus.Processing, video.Status);
        Assert.NotNull(video.UpdatedAt);
    }

    [Fact]
    public void MarkAsFailed_ShouldStoreReason()
    {
        // Arrange
        var video = new Video("Test", "file.mp4", 100, "path", Guid.NewGuid());
        var reason = "FFmpeg process timed out";

        // Act
        video.MarkAsFailed(reason);

        // Assert
        Assert.Equal(VideoStatus.Failed, video.Status);
        // If you added FailureReason property:
        // Assert.Equal(reason, video.FailureReason); 
    }
}