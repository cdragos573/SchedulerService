
using SchedulerService.Models;

namespace SchedulerService.Services;

public interface IJobSyncService
{
    Task SynchronizeJobsAsync(CancellationToken cancellationToken);
    Task SynchronizeJobsAsync(JobModel[] redisJobs, CancellationToken cancellationToken);
}