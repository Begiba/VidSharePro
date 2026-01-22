// VidSharePro.Domain/Entities/BackgroundJob.cs
using VidSharePro.Domain.Common;
using VidSharePro.Domain.Enums;

namespace VidSharePro.Domain.Entities;

public class BackgroundJob : BaseEntity
{
    public string JobType { get; private set; } = null!;
    public JobStatus Status { get; private set; }
    public string ReferenceId { get; private set; } = null!; // Usually VideoId
    public string? ErrorMessage { get; private set; }

    // Required for EF Core
    public BackgroundJob() { }

    // This is the 2-argument constructor the Service layer is looking for
    public BackgroundJob(string jobType, string referenceId)
    {
        if (string.IsNullOrWhiteSpace(jobType)) throw new ArgumentException("Job type is required");
        if (string.IsNullOrWhiteSpace(referenceId)) throw new ArgumentException("Reference ID is required");

        JobType = jobType;
        ReferenceId = referenceId;
        Status = JobStatus.Queued;
    }

    public void UpdateStatus(JobStatus status)
    {
        Status = status;
        MarkAsUpdated("System");
    }

    public void MarkAsFailed(string error)
    {
        Status = JobStatus.Failed;
        ErrorMessage = error;
        MarkAsUpdated("System");
    }
}