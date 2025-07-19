using Microsoft.AspNetCore.Mvc;
using ShiftLogger.API.Services;
using ShiftLogger.API.DTOs;


namespace ShiftLogger.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class WorkersController : ControllerBase
{
    private readonly IWorkerService _workerService;

    public WorkersController(IWorkerService workerService)
    {
        _workerService = workerService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkerResponse>>> GetAllWorkersAsync([FromQuery] string? name, CancellationToken cancellationToken)
    {
        var workersResult = string.IsNullOrEmpty(name)
            ? await _workerService.GetAllWorkersAsync(cancellationToken)
            : await _workerService.SearchWorkersByNameAsync(name, cancellationToken);

        return workersResult.IsSuccess
            ? Ok(workersResult.Value)
            : NotFound(workersResult.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkerResponse>> GetWorkerByIdAsync(int id, CancellationToken cancellationToken)
    {
        var workerResult = await _workerService.GetWorkerByIdAsync(id, cancellationToken);

        return workerResult.IsSuccess
            ? Ok(workerResult.Value)
            : NotFound(workerResult.ErrorMessage);
    }

    [HttpPost]
    public async Task<ActionResult<WorkerResponse>> CreateWorkerAsync(CreateWorkerRequest worker, CancellationToken cancellationToken)
    {
        var workerResult = await _workerService.CreateWorkerAsync(worker, cancellationToken);

        if (workerResult.IsSuccess)
        {
            var createdWorker = workerResult.Value;
            return Created($"/api/workers/{createdWorker?.WorkerId}", createdWorker);
        }

        return BadRequest(workerResult.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkerResponse>> UpdateWorkerAsync(int id, UpdateWorkerRequest worker, CancellationToken cancellationToken)
    {
        var workerResult = await _workerService.UpdateWorkerAsync(id, worker, cancellationToken);

        return workerResult.IsSuccess
            ? Ok(workerResult.Value)
            : BadRequest(workerResult.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWorkerAsync(int id, CancellationToken cancellationToken)
    {
        var workerResult = await _workerService.DeleteWorkerAsync(id, cancellationToken);

        return workerResult.IsSuccess
            ? NoContent()
            : BadRequest(workerResult.ErrorMessage);
    }
}
