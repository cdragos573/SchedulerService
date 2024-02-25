
namespace SchedulerService.Services;

public interface IQuartzJobMetricsService
{
    void RecordValue(long value, bool isSuccess, KeyValuePair<string, object?>[] tags);
}