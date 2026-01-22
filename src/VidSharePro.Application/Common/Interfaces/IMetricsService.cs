// VidSharePro.Application/Common/Interfaces/IMetricsService.cs
using System.Collections.Concurrent;

public interface IMetricsService
{
    void IncrementUploadCount();
    void RecordJobDuration(string jobType, double durationMs);
    void IncrementActiveStreams();
    void DecrementActiveStreams();
    object GetCurrentMetrics();
}

