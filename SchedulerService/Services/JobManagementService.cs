using System.Threading;
using SchedulerService.Models;

namespace SchedulerService.Services;

public class JobManagementService : IJobManagementService
{
    private readonly IRedisJobRepository _repository;
    private readonly IJobSyncService _syncService;

    public JobManagementService(IRedisJobRepository repository, IJobSyncService syncService)
    {
        _repository = repository;
        _syncService = syncService;
    }

    public async Task<JobModel[]> GetAllAsync(CancellationToken cancellationToken)
    {
        var data = await _repository.GetJobsAsync(cancellationToken);

        return data;
    }

    public async Task<JobModel?> GetAsync(string jobName, CancellationToken cancellationToken)
    {
        var data = await _repository.GetJobsAsync(cancellationToken);

        return data.FirstOrDefault(x => x.JobName == jobName);
    }

    public async Task<bool> CreateAsync(JobModel jobModel, CancellationToken cancellationToken)
    {
        var data = await _repository.GetJobsAsync(cancellationToken);
        if (data.Any(x => x.JobName == jobModel.JobName))
        {
            return false;
        }
        var newData = new JobModel[data.Length + 1];
        data.CopyTo(newData, 0);
        newData[data.Length] = jobModel;

        await _repository.SetJobsAsync(newData, cancellationToken);
        await _syncService.SynchronizeJobsAsync(newData, cancellationToken);

        return true;
    }

    public async Task<bool> UpdateAsync(JobModel jobModel, CancellationToken cancellationToken)
    {
        var data = await _repository.GetJobsAsync(cancellationToken);

        for (int i = 0; i < data.Length; i++)
        {
            if (data[i].JobName == jobModel.JobName)
            {
                data[i] = jobModel;
                await _repository.SetJobsAsync(data, cancellationToken);
                await _syncService.SynchronizeJobsAsync(data, cancellationToken);
                return true;
            }
        }

        return false;
    }

    public async Task<bool> DeleteAsync(string jobName, CancellationToken cancellationToken)
    {
        var data = await _repository.GetJobsAsync(cancellationToken);
        if (data.Length == 0)
        {
            return false;
        }

        var newData = new JobModel[data.Length - 1];
        int idx = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i].JobName != jobName)
            {
                if (idx >= newData.Length)
                {
                    return false;
                }
                newData[idx++] = data[i];
            }
        }

        await _repository.SetJobsAsync(newData, cancellationToken);
        await _syncService.SynchronizeJobsAsync(newData, cancellationToken);
        return true;
    }
}
