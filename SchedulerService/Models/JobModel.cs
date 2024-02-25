namespace SchedulerService.Models;

public class JobModel
{
    public string JobName { get; set; } = string.Empty;
    public string? JobDescription { get; set; }
    public string? CronExpressionString { get; set; }
    public string JobActionType { get; set; } = string.Empty;
    public Dictionary<string, string> JobData { get; set; } = new Dictionary<string, string>();
}
