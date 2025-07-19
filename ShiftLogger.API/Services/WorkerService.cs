using Microsoft.EntityFrameworkCore;
using ShiftLogger.API.Data;
using ShiftLogger.API.DTOs;
using ShiftLogger.API.Extensions;
using ShiftLogger.API.Models;
using ShiftLogger.API.Results;

namespace ShiftLogger.API.Services;

public class WorkerService : IWorkerService
{
    private readonly ShiftLoggerDbContext _context;
    private readonly ILogger<WorkerService> _logger;

    public WorkerService(ShiftLoggerDbContext context, ILogger<WorkerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<WorkerResponse>> CreateWorkerAsync(CreateWorkerRequest workerRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            if (workerRequest is null)
                return _logger.LogErrorAndReturnFail<DTOs.WorkerResponse>("Worker request cannot be null");

            var worker = new Worker
            {
                Name = workerRequest.Name,
                Department = workerRequest.Department,
                Email = workerRequest.Email,
                TelephoneNumber = workerRequest.TelephoneNumber,
            };

            await _context.Workers.AddAsync(worker, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<WorkerResponse>.Ok(MapToResponse(worker));
        }
        catch (DbUpdateException ex)
        {
            return _logger.LogErrorAndReturnFail<WorkerResponse>($"There was an error updating the database: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<WorkerResponse>($"An unexpected error occurred: {ex.Message}", ex);
        }

    }

    public async Task<Result<WorkerResponse>> UpdateWorkerAsync(int id, UpdateWorkerRequest workerRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return _logger.LogErrorAndReturnFail<WorkerResponse>($"Id = {id} is an invalid id. Ids must be greater than 0");

            if (workerRequest is null)
                return _logger.LogErrorAndReturnFail<WorkerResponse>("Update worker request cannot be null");

            var worker = await _context.Workers.FindAsync(id, cancellationToken);

            if (worker is null)
                return _logger.LogErrorAndReturnFail<WorkerResponse>($"No worker with id = {id} found in the database, nothing updated");

            worker.Name = workerRequest.Name!;
            worker.Department = workerRequest.Department!;
            worker.Email = workerRequest.Email;
            worker.TelephoneNumber = workerRequest.TelephoneNumber;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<WorkerResponse>.Ok(MapToResponse(worker));
        }
        catch (DbUpdateException ex)
        {
            return _logger.LogErrorAndReturnFail<WorkerResponse>($"There was an error updating the database: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<WorkerResponse>($"An unexpected error occurred: {ex.Message}", ex);
        }

    }

    public async Task<Result> DeleteWorkerAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return _logger.LogErrorAndReturnFail($"Id = {id} is an invalid id. Ids must be greater than 0");

            var workerToDelete = await _context.Workers
                .FindAsync(id, cancellationToken);

            if (workerToDelete is null)
                return _logger.LogErrorAndReturnFail($"There is no worker in the database with id = {id}");

            _context.Workers.Remove(workerToDelete);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return _logger.LogErrorAndReturnFail($"There was an error updating the database: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail($"An unexpected error occurred: {ex.Message}", ex);
        }

    }

    public async Task<Result<IReadOnlyList<WorkerResponse>>> GetAllWorkersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Workers.AsNoTracking();

            var workers = await query
                .Select(worker => MapToResponse(worker))
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<WorkerResponse>>.Ok(workers);
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<IReadOnlyList<WorkerResponse>>($"An unexpected error occurred while loading workers: {ex.Message}", ex);
        }

    }

    public async Task<Result<WorkerResponse>> GetWorkerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return _logger.LogErrorAndReturnFail<WorkerResponse>($"Id = {id} is an invalid id. Ids must be greater than 0");

            var query = _context.Workers.AsNoTracking();

            var worker = await query
                .FirstOrDefaultAsync(worker => worker.Id == id, cancellationToken);

            if (worker is null)
                return _logger.LogErrorAndReturnFail<WorkerResponse>($"There is no worker in the database with id = {id} ");


            return Result<WorkerResponse>.Ok(MapToResponse(worker));
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<WorkerResponse>($"An unexpected error occurred while loading workers: {ex.Message}", ex);
        }

    }

    public async Task<Result<IReadOnlyList<WorkerResponse>>> SearchWorkersByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return _logger.LogErrorAndReturnFail<IReadOnlyList<WorkerResponse>>("Worker's name cannot be null or blank");

            var query = _context.Workers.AsNoTracking();

            var workersByName = await query
                .Where(worker => worker.Name.ToLower().Contains(name.ToLower()))
                .Select(worker => MapToResponse(worker))
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<WorkerResponse>>.Ok(workersByName);
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<IReadOnlyList<WorkerResponse>>($"An unexpected error occurred while loading workers: {ex.Message}", ex);
        }

    }


    private static WorkerResponse MapToResponse(Worker worker) => new()
    {
        WorkerId = worker.Id,
        Name = worker.Name,
        Department = worker.Department,
        Email = worker.Email,
        TelephoneNumber = worker.TelephoneNumber,
    };


}
