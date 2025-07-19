using ShiftLogger.API.DTOs;
using ShiftLogger.API.Results;

namespace ShiftLogger.API.Services;

public interface IShiftService
{
    Task<Result<ShiftResponse>> CreateShiftAsync(CreateShiftRequest shiftRequest, CancellationToken cancellationToken = default);
    Task<Result<ShiftResponse>> UpdateShiftAsync(int shiftId, UpdateShiftRequest shift, CancellationToken cancellationToken = default);
    Task<Result> DeleteShiftAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ShiftResponse>> GetShiftByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ShiftResponse>>> GetAllShiftsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ShiftResponse>>> GetAllShiftsByWorkerId(int workerId, CancellationToken cancellationToken = default);
}
