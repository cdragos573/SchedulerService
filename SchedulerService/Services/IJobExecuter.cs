using Quartz;

namespace SchedulerService.Services;

public interface IJobExecuter
{
    Task ExecuteAsync(IJobExecutionContext context);
}
