using Quartz;
using System.Diagnostics;

namespace SchedulerService.Services;

public class RestJobExecuterStrategy : IJobExecuter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IQuartzJobMetricsService _metricsService;
    private readonly ILogger<RestJobExecuterStrategy> _logger;

    public RestJobExecuterStrategy(IHttpClientFactory httpClientFactory, 
        IQuartzJobMetricsService metricsService,
        ILogger<RestJobExecuterStrategy> logger)
    {
        _httpClientFactory = httpClientFactory;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task ExecuteAsync(IJobExecutionContext context)
    {
        string? verb = null;
        string? url = null;
        string? jobName = context.JobDetail.Key.Name;
        Stopwatch sw = Stopwatch.StartNew();
        Dictionary<string, object?> metricTags = new Dictionary<string, object?>()
        {
            { "actionType", "REST" }
        };
        bool isSuccess = false;

        try
        {
            using var cts = new CancellationTokenSource(2000);
            var httpClient = _httpClientFactory.CreateClient();
            verb = context.JobDetail.JobDataMap.GetString("verb");
            url = context.JobDetail.JobDataMap.GetString("URL");

            metricTags.Add("verb", verb);
            metricTags.Add("url", url);

            HttpResponseMessage? response = null;
            if (verb == "GET")
            {
                response = await httpClient.GetAsync(url, cts.Token);
            }
            else if (verb == "POST")
            {
                response = await httpClient.PostAsync(url, null, cts.Token);
            }
            else if (verb == "PUT")
            {
                response = await httpClient.PutAsync(url, null, cts.Token);
            }
            else if (verb == "DELETE")
            {
                response = await httpClient.DeleteAsync(url, cts.Token);
            }
            _logger.LogInformation($"RestJobExecuterStrategy executed Job: {jobName}, Verb: {verb}, URL: {url}, ResponseStatusCode:{response?.StatusCode ?? 0}");
            isSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred on Rest job trigger for Job: {jobName} Verb:{verb} URL:{url}", ex);
        }
        finally
        {
            sw.Stop();
            _metricsService.RecordValue(sw.ElapsedMilliseconds, isSuccess, metricTags.ToArray());
        }
    }
}
