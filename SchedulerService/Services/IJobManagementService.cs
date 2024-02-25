using SchedulerService.Models;

namespace SchedulerService.Services;
public interface IJobManagementService
{
    Task<bool> CreateAsync(JobModel jobModel, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string jobKey, CancellationToken cancellationToken);
    Task<JobModel[]> GetAllAsync(CancellationToken cancellationToken);
    Task<JobModel?> GetAsync(string jobName, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(JobModel jobModel, CancellationToken cancellationToken);
}