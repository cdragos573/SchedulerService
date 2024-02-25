using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchedulerService.Models;
using SchedulerService.Services;

namespace SchedulerService.Controllers;
[Route("api/[controller]")]
[ApiController]
public class JobsController : ControllerBase
{
    private readonly IJobManagementService _jobService;
    private readonly IJobSyncService _jobSyncService;
    private readonly IQuartzAdapterService _quartzAdapterService;

    public JobsController(IJobManagementService jobService, IJobSyncService jobSyncService, IQuartzAdapterService quartzAdapterService)
    {
        _jobService = jobService;
        _jobSyncService = jobSyncService;
        _quartzAdapterService = quartzAdapterService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var data = await _jobService.GetAllAsync(cancellationToken);

        return Ok(data);
    }

    [HttpGet, Route("fromQuartz")]
    public async Task<IActionResult> GetAllFromQuartzAsync(CancellationToken cancellationToken)
    {
        var data = await _quartzAdapterService.GetAllJobsAsync(cancellationToken);

        return Ok(data);
    }

    [HttpGet, Route("{jobName}")]
    public async Task<IActionResult> GetAsync(string jobName, CancellationToken cancellationToken)
    {
        var data = await _jobService.GetAsync(jobName, cancellationToken);
        if (data == null)
        {
            return NotFound();
        }

        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody]JobModel jobModel, CancellationToken cancellationToken)
    {
        var success = await _jobService.CreateAsync(jobModel, cancellationToken);

        if (!success)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] JobModel jobModel, CancellationToken cancellationToken)
    {
        var success = await _jobService.UpdateAsync(jobModel, cancellationToken);

        if (!success)
        {
            return BadRequest();
        }
        return NoContent();
    }
    
    [HttpDelete, Route("{jobName}")]
    public async Task<IActionResult> DeleteAsync(string jobName, CancellationToken cancellationToken)
    {
        var success = await _jobService.DeleteAsync(jobName, cancellationToken);

        if (!success)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpPost, Route("sync")]
    public async Task<IActionResult> SyncAsync(CancellationToken cancellationToken)
    {
        await _jobSyncService.SynchronizeJobsAsync(cancellationToken);

        return NoContent();
    }
}
