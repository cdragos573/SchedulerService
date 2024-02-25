using SchedulerService.Models;

namespace SchedulerService.Services;
public interface IRedisJobRepository
{
    Task<JobModel[]> GetJobsAsync(CancellationToken cancellationToken);
    Task SetJobsAsync(JobModel[] jobs, CancellationToken cancellationToken);
}