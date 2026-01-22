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
            // 1. Define the 'job' variable here
            var videoId = Guid.NewGuid();
            var job = new BackgroundJob("VideoValidation", videoId.ToString());
            //{
            //    Id = Guid.NewGuid(),
            //    JobType = "VideoValidation",
            //    ReferenceId = videoId.ToString(), // This links the job to the video
            //    Status = 0 // Pending
            //};

            var video = new Video("Title", "file.mp4", 1024, "path", Guid.NewGuid());
            // Use reflection or a helper to force 'Ready' status for the test if private
            // Or call the sequence: .CompleteUpload() -> .StartProcessing() -> .TransitionToReady()
            video.CompleteUpload();
            video.StartProcessing();
            video.TransitionToReady();

            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(video);
            // Use Reflection to find the private method
            var method = typeof(JobProcessorWorker)
                .GetMethod("HandleVideoValidation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            //await _worker.HandleVideoValidation(job, _serviceProvider, CancellationToken.None);
            // Invoke it
            var task = (Task)method.Invoke(_worker, new object[] { job, _serviceProvider, CancellationToken.None });
            await task;

            // Assert
            // Verify that ProcessingService was NEVER called because we returned early
            _processingServiceMock.Verify(p => p.ProcessAndGenerateThumbnailAsync(It.IsAny<Video>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
