using Microsoft.AspNetCore.Mvc;
using ShiftLogger.API.Services;
using ShiftLogger.API.DTOs;

namespace ShiftLogger.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShiftResponse>>> GetAllShiftsAsync(CancellationToken cancellationToken)
    {
        var shiftResult = await _shiftService.GetAllShiftsAsync(cancellationToken);

        return shiftResult.IsSuccess
            ? Ok(shiftResult.Value)
            : NotFound(shiftResult.ErrorMessage);
    }

    [HttpGet("worker/{workerId}")]
    public async Task<ActionResult<IReadOnlyList<ShiftResponse>>> GetAllShiftsByWorkerIdAsync(int workerId, CancellationToken cancellationToken)
    {
        var shiftResult = await _shiftService.GetAllShiftsByWorkerId(workerId, cancellationToken);

        return shiftResult.IsSuccess
            ? Ok(shiftResult.Value)
            : NotFound(shiftResult.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShiftResponse>> GetShiftByIdAsync(int id, CancellationToken cancellationToken)
    {
        var shiftResult = await _shiftService.GetShiftByIdAsync(id, cancellationToken);

        return shiftResult.IsSuccess
            ? Ok(shiftResult.Value)
            : NotFound(shiftResult.ErrorMessage);
    }

    [HttpPost]
    public async Task<ActionResult<ShiftResponse>> CreateShiftAsync(CreateShiftRequest shiftRequest, CancellationToken cancellationToken)
    {
        var shiftResult = await _shiftService.CreateShiftAsync(shiftRequest, cancellationToken);

        if (shiftResult.IsSuccess)
        {
            var createdShift = shiftResult.Value;
            return Created($"/api/shifts/{createdShift?.Id}", createdShift);
        }

        return BadRequest(shiftResult.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ShiftResponse>> UpdateShiftAsync(int id, UpdateShiftRequest shiftRequest, CancellationToken cancellationToken)
    {
        var shiftResult = await _shiftService.UpdateShiftAsync(id, shiftRequest, cancellationToken);

        return shiftResult.IsSuccess
            ? Ok(shiftResult.Value)
            : BadRequest(shiftResult.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteShiftAsync(int id, CancellationToken cancellationToken)
    {
        var shiftResult = await _shiftService.DeleteShiftAsync(id, cancellationToken);

        return shiftResult.IsSuccess
            ? NoContent()
            : BadRequest(shiftResult.ErrorMessage);
    }
}
