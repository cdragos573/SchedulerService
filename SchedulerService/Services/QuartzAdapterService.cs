using Quartz;
using Quartz.Impl.Matchers;
using SchedulerService.Models;

namespace SchedulerService.Services;

public class QuartzAdapterService : IQuartzAdapterService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private const string ActionTypeKey = "type";
    private const string DefaultActionType = "REST";

    public QuartzAdapterService(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public async Task<JobModel[]> GetAllJobsAsync(CancellationToken cancellationToken)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobs = new List<JobModel>();

        var keys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), cancellationToken);
        foreach (var key in keys)
        {

            var job = await scheduler.GetJobDetail(key, cancellationToken);
            string? actionType = job.JobDataMap.GetString(ActionTypeKey) ?? DefaultActionType;
            var dataMap = job.JobDataMap.Where(x => x.Key != ActionTypeKey).ToDictionary(x => x.Key, x => x.Value.ToString());

            var jobData = new JobModel
            {
                JobName = job.Key.Name,
                JobDescription = job.Description,
                JobActionType = actionType,
                JobData = dataMap
            };
            var triggers = await scheduler.GetTriggersOfJob(key);
            var trigger = triggers.FirstOrDefault();
            if (trigger is ICronTrigger cronTrigger)
            {
                jobData.CronExpressionString = cronTrigger.CronExpressionString;
            }
            jobs.Add(jobData);
        }

        return jobs.ToArray();
    }

    public async Task<string[]> GetAllJobKeysAsync(CancellationToken cancellationToken)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var keys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), cancellationToken);

        return keys.Select(x => x.Name).ToArray();
    }

    public async Task DeleteJobsAsync(string[] jobKeys, CancellationToken cancellationToken)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var keys = jobKeys.Select(x => new JobKey(x)).ToArray();
        await scheduler.DeleteJobs(keys, cancellationToken);
    }

    public async Task SetJobAsync(JobModel jobModel, CancellationToken cancellationToken)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var key = new JobKey(jobModel.JobName);

        var jobData = jobModel.JobData.ToDictionary(x => x.Key, x => x.Value);
        jobData["type"] = jobModel.JobActionType;

        bool updateJob = false;
        var job = await scheduler.GetJobDetail(key, cancellationToken);

        if (job == null || ShouldUpdateJob(jobModel, jobData, job))
        {
            updateJob = true;
        }
        else
        {
            var trigger = await scheduler.GetTrigger(new TriggerKey($"{jobModel.JobName}-trigger"), cancellationToken);
            if (trigger == null)
            {
                updateJob = true;
            }
            else if (trigger is ICronTrigger cronTrigger)
            {
                updateJob |= jobModel.CronExpressionString != cronTrigger.CronExpressionString;
            }
        }

        if (updateJob)
        {
            if (job != null)
            {
                await scheduler.DeleteJob(key, cancellationToken);
            }

            var newJob = JobBuilder.Create<GenericQuartzJob>()
                    .WithIdentity(key)
                    .WithDescription(jobModel.JobDescription)
                    .SetJobData(new JobDataMap((IDictionary<string, object>)jobData.ToDictionary(x => x.Key, x => (object)x.Value)))
                    .Build();
            var newTrigger =
                TriggerBuilder.Create()
                .WithIdentity($"{jobModel.JobName}-trigger")
                .WithCronSchedule(jobModel.CronExpressionString!)
                .StartNow()
                .Build();
            await scheduler.ScheduleJob(newJob, newTrigger, cancellationToken);
        }
    }

    private static bool ShouldUpdateJob(JobModel jobModel, IDictionary<string, string> modelJobData, IJobDetail job)
    {
        try
        {
            if (jobModel.JobDescription != job.Description)
            {
                return true;
            }

            if (modelJobData.Count != job.JobDataMap.Count)
            {
                return true;
            }

            foreach (var key in modelJobData.Keys)
            {
                if (modelJobData[key] != job.JobDataMap.GetString(key))
                {
                    return true;
                }
            }
        }
        catch
        {
            return true;
        }

        return false;
    }
}
