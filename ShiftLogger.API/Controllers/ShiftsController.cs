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
    public async Task<ActionResult<IReadOnlyList<ShiftResponse>>> GetAllShiftsAsync()
    {
        var shiftResult = await _shiftService.GetAllShiftsAsync();

        return shiftResult.IsSuccess
            ? Ok(shiftResult.Value)
            : NotFound(shiftResult.ErrorMessage);
    }

    [HttpGet("worker/{workerId}")]
    public async Task<ActionResult<IReadOnlyList<ShiftResponse>>> GetAllShiftsByWorkerIdAsync(int workerId)
    {
        var shiftResult = await _shiftService.GetAllShiftsByWorkerId(workerId);

        return shiftResult.IsSuccess
            ? Ok(shiftResult.Value)
            : NotFound(shiftResult.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShiftResponse>> GetShiftByIdAsync(int id)
    {
        var shiftResult = await _shiftService.GetShiftByIdAsync(id);

        return shiftResult.IsSuccess
            ? Ok(shiftResult.Value)
            : NotFound(shiftResult.ErrorMessage);
    }

    [HttpPost]
    public async Task<ActionResult<ShiftResponse>> CreateShiftAsync(CreateShiftRequest shiftRequest)
    {
        var shiftResult = await _shiftService.CreateShiftAsync(shiftRequest);

        if (shiftResult.IsSuccess)
        {
            var createdShift = shiftResult.Value;
            return Created($"/api/shifts/{createdShift?.Id}", createdShift);
        }

        return BadRequest(shiftResult.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ShiftResponse>> UpdateShiftAsync(int id, UpdateShiftRequest shiftRequest)
    {
        var shiftResult = await _shiftService.UpdateShiftAsync(id, shiftRequest);

        return shiftResult.IsSuccess
            ? Ok(shiftResult.Value)
            : BadRequest(shiftResult.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteShiftAsync(int id)
    {
        var shiftResult = await _shiftService.DeleteShiftAsync(id);

        return shiftResult.IsSuccess
            ? NoContent()
            : BadRequest(shiftResult.ErrorMessage);
    }
}
