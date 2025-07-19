using ShiftLogger.API.Results;
using ShiftLogger.API.DTOs;

namespace ShiftLogger.API.Services;

public interface IWorkerService
{
    Task<Result<WorkerResponse>> CreateWorkerAsync(CreateWorkerRequest workerRequest, CancellationToken cancellationToken = default);
    Task<Result<WorkerResponse>> UpdateWorkerAsync(int id, UpdateWorkerRequest workerRequest, CancellationToken cancellationToken = default);
    Task<Result> DeleteWorkerAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<WorkerResponse>> GetWorkerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<WorkerResponse>>> SearchWorkersByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<WorkerResponse>>> GetAllWorkersAsync(CancellationToken cancellationToken = default);
}
