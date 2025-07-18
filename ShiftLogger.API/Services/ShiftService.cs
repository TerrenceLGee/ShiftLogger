using Microsoft.EntityFrameworkCore;
using ShiftLogger.API.Data;
using ShiftLogger.API.DTOs;
using ShiftLogger.API.Extensions;
using ShiftLogger.API.Models;
using ShiftLogger.API.Results;

namespace ShiftLogger.API.Services;

public class ShiftService : IShiftService
{
    private readonly ShiftLoggerDbContext _context;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(ShiftLoggerDbContext context, ILogger<ShiftService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ShiftResponse>> CreateShiftAsync(CreateShiftRequest shiftRequest)
    {
        try
        {
            if (shiftRequest is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>("Shift request cannot be null");

            if (shiftRequest.StartTime >= shiftRequest.EndTime)
                return _logger.LogErrorAndReturnFail<ShiftResponse>("Start time must come before end time");

            var worker = await _context.Workers.FindAsync(shiftRequest.WorkerId);
            if (worker is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"There is no worker in the database with id = {shiftRequest.WorkerId}");

            var shift = new Shift
            {
                WorkerId = shiftRequest.WorkerId,
                Worker = worker,                
                StartTime = shiftRequest.StartTime,
                EndTime = shiftRequest.EndTime,
            };

            await _context.Shifts.AddAsync(shift);

            await _context.SaveChangesAsync();

            return Result<ShiftResponse>.Ok(MapToShiftResponse(shift, worker));
        }
        catch (DbUpdateException ex)
        {
            return _logger.LogErrorAndReturnFail<ShiftResponse>($"There was an error updating the database: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<ShiftResponse>($"An unexpected error occurred: {ex.Message}", ex);
        }
    }

    public async Task<Result<ShiftResponse>> UpdateShiftAsync(int shiftId, UpdateShiftRequest shiftRequest)
    {
        try
        {
            if (shiftId <= 0)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"Shift id = {shiftId} is invalid, shift ids must be greater than 0");

            if (shiftRequest is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>("Shift request cannot be null");

            if (shiftRequest.StartTime >= shiftRequest.EndTime)
                return _logger.LogErrorAndReturnFail<ShiftResponse>("Start time must come before end time");

            var shift = await _context.Shifts.FindAsync(shiftId);

            if (shift is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"There is no shift with id = {shiftId} available in the database, nothing updated");

            var worker = await _context.Workers.FindAsync(shiftRequest.WorkerId);

            if (worker is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"There is no worker in the database with id = {shiftRequest.WorkerId}");

            shift.WorkerId = shiftRequest.WorkerId;
            shift.Worker = worker;
            shift.StartTime = shiftRequest.StartTime;
            shift.EndTime = shiftRequest.EndTime;

            await _context.SaveChangesAsync();

            return Result<ShiftResponse>.Ok(MapToShiftResponse(shift, worker));

        }
        catch (DbUpdateException ex)
        {
            return _logger.LogErrorAndReturnFail<ShiftResponse>($"There was an error updating the database: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<ShiftResponse>($"An unexpected error occurred: {ex.Message}", ex);
        }

    }

    public async Task<Result> DeleteShiftAsync(int id)
    {
        try
        {
            if (id <= 0)
                return _logger.LogErrorAndReturnFail($"Id = {id} is invalid, shift ids must be greater than 0");

            var shiftToDelete = await _context.Shifts
               .FindAsync(id);

            if (shiftToDelete is null)
                return _logger.LogErrorAndReturnFail($"Shift with id = {id} is not in the database, nothing deleted");

            _context.Shifts.Remove(shiftToDelete);

            await _context.SaveChangesAsync();

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

    public async Task<Result<IReadOnlyList<ShiftResponse>>> GetAllShiftsAsync()
    {

        try
        {
            var shifts = await _context.Shifts
                .AsNoTracking()
                .Include(shift => shift.Worker)
                .ToListAsync();

            var shiftResponses = shifts
                .Select(shift => MapToShiftResponse(shift, _context.Workers.Find(shift.WorkerId)!))
                .ToList();

            return Result<IReadOnlyList<ShiftResponse>>.Ok(shiftResponses);

        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<IReadOnlyList<ShiftResponse>>($"An unexpected error occurred loading shifts: {ex.Message}", ex);
        }

    }

    public async Task<Result<IReadOnlyList<ShiftResponse>>> GetAllShiftsByWorkerId(int workerId)
    {
        try
        {
            if (workerId <= 0)
                return _logger.LogErrorAndReturnFail<IReadOnlyList<ShiftResponse>>($"Worker id = {workerId} is invalid, worker ids must be greater than 0");

            var query = _context.Shifts.AsNoTracking();

            var shifts = await _context.Shifts
                .AsNoTracking()
                .Where(shift => shift.WorkerId == workerId)
                .Include(shift => shift.Worker)
                .ToListAsync();

            var shiftResponses = shifts
                .Select(shift => MapToShiftResponse(shift, _context.Workers.Find(shift.WorkerId)!))
                .ToList();

            return Result<IReadOnlyList<ShiftResponse>>.Ok(shiftResponses);

        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<IReadOnlyList<ShiftResponse>>($"An unexpected error occurred loading shifts: {ex.Message}", ex);
        }

    }

    public async Task<Result<ShiftResponse>> GetShiftByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"Shift id = {id} is invalid, shift ids must be greater than 0");

            var shift = await _context.Shifts
                .FirstOrDefaultAsync(shift => shift.Id == id);

            if (shift is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"There is no shift available in the database with id = {id}");

            var worker = await _context.Workers.FindAsync(shift.WorkerId);

            if (worker is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"There is no worker in the database with id = {shift.WorkerId}");

            return Result<ShiftResponse>.Ok(MapToShiftResponse(shift, worker));

        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<ShiftResponse>($"An unexpected error occurred loading shifts: {ex.Message}", ex);
        }

    }

    private static ShiftResponse MapToShiftResponse(Shift shift, Worker worker) => new()
    {
        Id = shift.Id,
        WorkerId = worker.Id,
        WorkerName = worker.Name,
        WorkerDepartment = worker.Department,
        StartTime = shift.StartTime,
        EndTime = shift.EndTime,
        Duration = shift.Duration,
    };

}
