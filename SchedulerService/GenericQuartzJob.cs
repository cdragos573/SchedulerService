using Quartz;
using SchedulerService.Services;

namespace SchedulerService;

public class GenericQuartzJob : IJob
{
    private readonly IJobExecuter _restExecuter;
    private readonly IJobExecuter _massTransitExecuter;

    public GenericQuartzJob(
        [FromKeyedServices("REST")] IJobExecuter restExecuter,
        [FromKeyedServices("MassTransit")] IJobExecuter massTransitExecuter
        )
    {
        _restExecuter = restExecuter;
        _massTransitExecuter = massTransitExecuter;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var actionType = context.JobDetail.JobDataMap.GetString("type");
        var executer = actionType switch
        {
            "REST" => _restExecuter,
            "MassTransit" => _massTransitExecuter,
            _ => default
        };
        if (executer == null )
        {
            Console.WriteLine("Invalid action type");
            return;
        }
        await executer.ExecuteAsync(context);
    }
}