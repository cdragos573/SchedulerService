using Quartz;

namespace SchedulerService.Services;

public class MassTransitJobExecuterStrategy : IJobExecuter
{
    public async Task ExecuteAsync(IJobExecutionContext context)
    {
        Console.WriteLine("MassTransitJobExecuterStrategy exec");
    }
}
