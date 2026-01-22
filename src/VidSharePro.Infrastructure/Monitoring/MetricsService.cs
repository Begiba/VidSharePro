// VidSharePro.Infrastructure/Monitoring/MetricsService.cs
using System.Collections.Concurrent;

namespace VidSharePro.Infrastructure.Monitoring;
public class MetricsService : IMetricsService
{
    private long _totalUploads = 0;
    private long _activeStreams = 0;
    private readonly ConcurrentDictionary<string, List<double>> _jobDurations = new();

    public void IncrementUploadCount() => Interlocked.Increment(ref _totalUploads);
    public void IncrementActiveStreams() => Interlocked.Increment(ref _activeStreams);
    public void DecrementActiveStreams() => Interlocked.Decrement(ref _activeStreams);

    public void RecordJobDuration(string jobType, double durationMs)
    {
        var list = _jobDurations.GetOrAdd(jobType, _ => new List<double>());
        lock (list) { list.Add(durationMs); }
    }

    public object GetCurrentMetrics() => new
    {
        TotalUploads = _totalUploads,
        ActiveStreams = _activeStreams,
        JobStats = _jobDurations.ToDictionary(k => k.Key, v => v.Value.DefaultIfEmpty(0).Average())
    };
}