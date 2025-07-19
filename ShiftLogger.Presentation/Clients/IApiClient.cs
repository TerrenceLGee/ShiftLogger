using ShiftLogger.API.DTOs;
using ShiftLogger.API.Results;

namespace ShiftLogger.Presentation.Clients;

public interface IApiClient
{
    // Worker endpoints
    Task<Result<WorkerResponse>> CreateWorkerAsync(CreateWorkerRequest request, CancellationToken cancellationToken = default);
    Task<Result<WorkerResponse>> UpdateWorkerAsync(int id, UpdateWorkerRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteWorkerAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<WorkerResponse>> GetWorkerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<WorkerResponse>>> GetWorkersAsync(string? nameFilter = null, CancellationToken cancellationToken = default);

    // Shift endpoints
    Task<Result<ShiftResponse>> CreateShiftAsync(CreateShiftRequest request, CancellationToken cancellationToken = default);
    Task<Result<ShiftResponse>> UpdateShiftAsync(int id, UpdateShiftRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteShiftAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ShiftResponse>> GetShiftByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ShiftResponse>>> GetShiftsAsync(int? workerId, CancellationToken cancellationToken = default);
}
