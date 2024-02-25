using SchedulerService.Models;

namespace SchedulerService.Services;

public class JobSyncService : IJobSyncService
{
    private readonly IQuartzAdapterService _quartzAdapterService;
    private readonly IRedisJobRepository _redisJobRepository;

    public JobSyncService(IQuartzAdapterService quartzAdapterService, IRedisJobRepository redisJobRepository)
    {
        _quartzAdapterService = quartzAdapterService;
        _redisJobRepository = redisJobRepository;
    }

    public async Task SynchronizeJobsAsync(CancellationToken cancellationToken)
    {
        var redisJobsTask = _redisJobRepository.GetJobsAsync(cancellationToken);
        var quartzJobKeysTask = _quartzAdapterService.GetAllJobKeysAsync(cancellationToken);
        await Task.WhenAll(redisJobsTask, quartzJobKeysTask);
        var redisJobs = await redisJobsTask;
        var quartzJobKeys = await quartzJobKeysTask;

        var keysToDelete = quartzJobKeys
            .Except(redisJobs.Select(x => x.JobName).ToArray())
            .ToArray();
        await _quartzAdapterService.DeleteJobsAsync(keysToDelete, cancellationToken);
        foreach (var job in redisJobs)
        {
            await _quartzAdapterService.SetJobAsync(job, cancellationToken);
        }
    }

    public async Task SynchronizeJobsAsync(JobModel[] redisJobs, CancellationToken cancellationToken)
    {
        var quartzJobKeys = await _quartzAdapterService.GetAllJobKeysAsync(cancellationToken);
        
        var keysToDelete = quartzJobKeys
            .Except(redisJobs.Select(x => x.JobName).ToArray())
            .ToArray();
        await _quartzAdapterService.DeleteJobsAsync(keysToDelete, cancellationToken);
        foreach (var job in redisJobs)
        {
            await _quartzAdapterService.SetJobAsync(job, cancellationToken);
        }
    }
}
