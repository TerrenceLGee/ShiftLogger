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
    public async Task<ActionResult<IReadOnlyList<WorkerResponse>>> GetAllWorkersAsync([FromQuery] string? name)
    {
        var workersResult = string.IsNullOrEmpty(name)
            ? await _workerService.GetAllWorkersAsync()
            : await _workerService.SearchWorkersByNameAsync(name);

        return workersResult.IsSuccess
            ? Ok(workersResult.Value)
            : NotFound(workersResult.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkerResponse>> GetWorkerByIdAsync(int id)
    {
        var workerResult = await _workerService.GetWorkerByIdAsync(id);

        return workerResult.IsSuccess
            ? Ok(workerResult.Value)
            : NotFound(workerResult.ErrorMessage);
    }

    [HttpPost]
    public async Task<ActionResult<WorkerResponse>> CreateWorkerAsync(CreateWorkerRequest worker)
    {
        var workerResult = await _workerService.CreateWorkerAsync(worker);

        if (workerResult.IsSuccess)
        {
            var createdWorker = workerResult.Value;
            return Created($"/api/workers/{createdWorker?.WorkerId}", createdWorker);
        }

        return BadRequest(workerResult.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkerResponse>> UpdateWorkerAsync(int id, UpdateWorkerRequest worker)
    {
        var workerResult = await _workerService.UpdateWorkerAsync(id, worker);

        return workerResult.IsSuccess
            ? Ok(workerResult.Value)
            : BadRequest(workerResult.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWorkerAsync(int id)
    {
        var workerResult = await _workerService.DeleteWorkerAsync(id);

        return workerResult.IsSuccess
            ? NoContent()
            : BadRequest(workerResult.ErrorMessage);
    }
}
