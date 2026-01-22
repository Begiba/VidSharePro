using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Application.Services;
using VidSharePro.Domain.Entities;

namespace VidSharePro.Tests.Application
{
    public class JobProcessorTests
    {
        private readonly Mock<IVideoRepository> _repoMock;
        private readonly Mock<IVideoProcessingService> _processingServiceMock;
        private readonly IServiceProvider _serviceProvider;
        private readonly JobProcessorWorker _worker;
        private readonly Mock<ILogger<JobProcessorWorker>> _loggerMock;


        public JobProcessorTests()
        {
            _repoMock = new Mock<IVideoRepository>();
            _processingServiceMock = new Mock<IVideoProcessingService>();
            _loggerMock = new Mock<ILogger<JobProcessorWorker>>();
            var services = new ServiceCollection();
            services.AddSingleton(_repoMock.Object);
            services.AddSingleton(_processingServiceMock.Object);
            _serviceProvider = services.BuildServiceProvider();
            _worker = new JobProcessorWorker(_serviceProvider, _loggerMock.Object);

        }

        [Fact]
        public async Task HandleVideoValidation_ShouldReturnEarly_IfVideoAlreadyReady()
        {
            // Arrange
            var videoId = Guid.NewGuid();
            var job = new BackgroundJob("VideoValidation", videoId.ToString());

            var video = new Video("Title", "file.mp4", 1024, "path", Guid.NewGuid());

            // 1. Move through the required states
            video.CompleteUpload();
            video.StartProcessing();

            // 2. ADD THIS: Satisfy the Domain Guard Clause
            // Assuming you have a method like AddFormat or a collection you can access
            video.AddFormat(
                "1080p",
                "storage/videos/1080.mp4",
                "video/mp4",
                5000000 // example 5MB size
            );

            // 3. Now this will no longer throw the InvalidOperationException
            video.TransitionToReady();

            _repoMock.Setup(r => r.GetByIdAsync(videoId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(video);
            var method = typeof(JobProcessorWorker)
        .GetMethod("HandleVideoValidation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            //await _worker.HandleVideoValidation(job, _serviceProvider, CancellationToken.None);
            var task = (Task)method.Invoke(_worker, new object[] { job, _serviceProvider, CancellationToken.None });
            await task;

            // Assert
            _processingServiceMock.Verify(p => p.ProcessAndGenerateThumbnailAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
