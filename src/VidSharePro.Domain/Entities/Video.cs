// VidSharePro.Domain/Entities/Video.cs
using VidSharePro.Domain.Common;
using VidSharePro.Domain.Enums;

namespace VidSharePro.Domain.Entities;

public class Video : BaseEntity
{
    public string Title { get; private set; } = null!;
    public string OriginalFileName { get; private set; } = null!;
    public long FileSizeInBytes { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public VideoStatus Status { get; private set; } = VideoStatus.Uploading;
    
    public string? FailureReason { get; private set; } // Add this property
    public Guid OwnerId { get; private set; }

    private readonly List<VideoFormat> _formats = new();
    public IReadOnlyCollection<VideoFormat> Formats => _formats.AsReadOnly();

    // 1. Add this for EF Core
    private Video() { }

    public Video(string title, string fileName, long size, string storagePath, Guid ownerId)
    {
        Title = title;
        OriginalFileName = fileName;
        FileSizeInBytes = size;
        StoragePath = storagePath;
        OwnerId = ownerId;
        Status = VideoStatus.Uploading;
    }

    public void CompleteUpload()
    {
        if (Status != VideoStatus.Uploading)
            throw new InvalidOperationException("Video can only be completed from the Uploading state.");

        Status = VideoStatus.PendingValidation;
        UpdateModifiedDate();
    }
    public void MarkAsReady() => Status = VideoStatus.Ready;

    public void AddFormat(string label, string path, string mimeType, long size)
    {
        _formats.Add(new VideoFormat(Id, label, path, mimeType, size));
    }

    // Add this method so the worker can update the path after conversion
    public void UpdateStoragePath(string newPath)
    {
        StoragePath = newPath;
    }

    public void TransitionToValidation()
    {
        if (Status != VideoStatus.Uploading)
            throw new InvalidOperationException($"Cannot move to Validation from {Status}");

        Status = VideoStatus.PendingValidation;
    }

    public void TransitionToReady()
    {

        // Business Rule: Video must be in a validation state before it can be marked as Ready.
        if (Status != VideoStatus.PendingValidation && Status != VideoStatus.Processing)
            throw new InvalidOperationException($"Video must be validated before it can be marked as Ready. Current status: {Status}");

        if (!_formats.Any())
            throw new InvalidOperationException("Video cannot be Ready without at least one processed format.");

        Status = VideoStatus.Ready;
        UpdateModifiedDate();
    }

    public void MarkAsFailed(string reason)
    {
        // Failures can happen during upload or validation
        Status = VideoStatus.Failed;
        this.FailureReason = reason; // Store the why
        // Logic could be added here to log the failure reason in a Domain Event
        UpdateModifiedDate();
    }

    public void MarkAsDeleted() => Status = VideoStatus.Deleted;

    public void MarkAsPendingValidation()
    {
        // Only allow this if we are currently in the initial 'Uploading' state
        if (Status != VideoStatus.Uploading)
            return;

        this.Status = VideoStatus.PendingValidation;

        // This is where we update the timestamp internally
        this.UpdateModifiedDate();
    }

    public void StartProcessing()
    {
        // Guard clause: Only allow processing if it's currently pending
        if (Status != VideoStatus.PendingValidation && Status != VideoStatus.Uploading)
        {
            throw new InvalidOperationException($"Cannot start processing. Current status is {Status}");
        }

        this.Status = VideoStatus.Processing;
        this.UpdateModifiedDate();
    }
}

