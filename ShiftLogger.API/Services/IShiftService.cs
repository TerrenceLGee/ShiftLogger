using ShiftLogger.API.DTOs;
using ShiftLogger.API.Results;

namespace ShiftLogger.API.Services;

public interface IShiftService
{
    Task<Result<ShiftResponse>> CreateShiftAsync(CreateShiftRequest shiftRequest);
    Task<Result<ShiftResponse>> UpdateShiftAsync(int shiftId, UpdateShiftRequest shift);
    Task<Result> DeleteShiftAsync(int id);
    Task<Result<ShiftResponse>> GetShiftByIdAsync(int id);
    Task<Result<IReadOnlyList<ShiftResponse>>> GetAllShiftsAsync();
    Task<Result<IReadOnlyList<ShiftResponse>>> GetAllShiftsByWorkerId(int workerId);
}
