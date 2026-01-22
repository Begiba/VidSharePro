using VidSharePro.Domain.Common;
using VidSharePro.Domain.Enums;

namespace VidSharePro.Domain.Entities;
// VidSharePro.Domain/Entities/Job.cs
public class Job : BaseEntity
{
    public string JobType { get; private set; } = null!;
    public JobStatus Status { get; private set; }
    public string ReferenceId { get; private set; } = null!; // Usually VideoId

    public Job(string type, string referenceId)
    {
        JobType = type;
        ReferenceId = referenceId;
        Status = JobStatus.Queued;
    }

    public void UpdateStatus(JobStatus status) => Status = status;
}