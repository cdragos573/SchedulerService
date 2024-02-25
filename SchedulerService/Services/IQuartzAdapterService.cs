using SchedulerService.Models;

namespace SchedulerService.Services;
public interface IQuartzAdapterService
{
    Task DeleteJobsAsync(string[] jobKeys, CancellationToken cancellationToken);
    Task<string[]> GetAllJobKeysAsync(CancellationToken cancellationToken);
    Task<JobModel[]> GetAllJobsAsync(CancellationToken cancellationToken);
    Task SetJobAsync(JobModel jobModel, CancellationToken cancellationToken);
}