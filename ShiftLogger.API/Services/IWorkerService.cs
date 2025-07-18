using ShiftLogger.API.Results;
using ShiftLogger.API.DTOs;

namespace ShiftLogger.API.Services;

public interface IWorkerService
{
    Task<Result<WorkerResponse>> CreateWorkerAsync(CreateWorkerRequest workerRequest);
    Task<Result<WorkerResponse>> UpdateWorkerAsync(int id, UpdateWorkerRequest workerRequest);
    Task<Result> DeleteWorkerAsync(int id);
    Task<Result<WorkerResponse>> GetWorkerByIdAsync(int id);
    Task<Result<IReadOnlyList<WorkerResponse>>> SearchWorkersByNameAsync(string name);
    Task<Result<IReadOnlyList<WorkerResponse>>> GetAllWorkersAsync();
}
