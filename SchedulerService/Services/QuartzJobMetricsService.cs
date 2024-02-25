using System.Diagnostics.Metrics;

namespace SchedulerService.Services;

public class QuartzJobMetricsService : IQuartzJobMetricsService
{
    private readonly Histogram<long> _histogram;
    private readonly Counter<long> _errorCounter;

    public QuartzJobMetricsService(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Custom.Metrics");
        _histogram = meter.CreateHistogram<long>("GenericQuartzJob", description: "Generic Quartz job triggered");
        _errorCounter = meter.CreateCounter<long>("GenericQuartzJob_error", description: "Generic Quartz job error");
    }

    public void RecordValue(long value, bool isSuccess, KeyValuePair<string, object?>[] tags)
    {
        if (isSuccess)
        {
            _histogram.Record(value, tags);
        }
        else
        {
            _errorCounter.Add(1, tags);
        }
    }
}
