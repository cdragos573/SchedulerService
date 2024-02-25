using StackExchange.Redis;
using System.Text.Json;
using SchedulerService.Models;

namespace SchedulerService.Services;

public class RedisJobRepository : IRedisJobRepository
{
    private const string JobsRedisKey = "QuartzScheduler:Jobs";
    private readonly IDatabase _database;

    public RedisJobRepository(IDatabase database)
    {
        _database = database;
    }

    public async Task<JobModel[]> GetJobsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var data = await _database.StringGetAsync(new RedisKey(JobsRedisKey));
        if (data.IsNull)
        {
            return new JobModel[0];
        }
        return JsonSerializer.Deserialize<JobModel[]>(data!) ?? new JobModel[0];
    }

    public async Task SetJobsAsync(JobModel[] jobs, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sortedJobs = jobs.OrderBy(x => x.JobName).ToArray();
        var data = JsonSerializer.Serialize(jobs);
        await _database.StringSetAsync(new RedisKey(JobsRedisKey), new RedisValue(data));
    }
}
