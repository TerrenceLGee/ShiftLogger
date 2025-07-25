﻿using Microsoft.EntityFrameworkCore;
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

    public async Task<Result<ShiftResponse>> CreateShiftAsync(CreateShiftRequest shiftRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            if (shiftRequest is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>("Shift request cannot be null");

            if (shiftRequest.StartTime >= shiftRequest.EndTime)
                return _logger.LogErrorAndReturnFail<ShiftResponse>("Start time must come before end time");

            var worker = await _context.Workers.FindAsync(shiftRequest.WorkerId, cancellationToken);
            if (worker is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"There is no worker in the database with id = {shiftRequest.WorkerId}");

            var shift = new Shift
            {
                WorkerId = shiftRequest.WorkerId,            
                StartTime = shiftRequest.StartTime,
                EndTime = shiftRequest.EndTime,
            };

            await _context.Shifts.AddAsync(shift, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var fullShift = await _context.Shifts
                .Include(shift => shift.Worker)
                .FirstAsync(s => s.Id == shift.Id, cancellationToken);

            return Result<ShiftResponse>.Ok(MapToShiftResponse(fullShift));
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

    public async Task<Result<ShiftResponse>> UpdateShiftAsync(int shiftId, UpdateShiftRequest shiftRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            if (shiftId <= 0)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"Shift id = {shiftId} is invalid, shift ids must be greater than 0");

            if (shiftRequest is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>("Shift request cannot be null");

            if (shiftRequest.StartTime >= shiftRequest.EndTime)
                return _logger.LogErrorAndReturnFail<ShiftResponse>("Start time must come before end time");

            var shift = await _context.Shifts
                .Include(shift => shift.Worker)
                .FirstAsync(shift => shift.Id == shiftId, cancellationToken);

            if (shift is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"There is no shift with id = {shiftId} available in the database, nothing updated");

            var worker = await _context.Workers.FindAsync(shiftRequest.WorkerId, cancellationToken);

            if (worker is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"There is no worker in the database with id = {shiftRequest.WorkerId}");

            shift.WorkerId = shiftRequest.WorkerId;
            shift.StartTime = shiftRequest.StartTime;
            shift.EndTime = shiftRequest.EndTime;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<ShiftResponse>.Ok(MapToShiftResponse(shift));

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

    public async Task<Result> DeleteShiftAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return _logger.LogErrorAndReturnFail($"Id = {id} is invalid, shift ids must be greater than 0");

            var shiftToDelete = await _context.Shifts
               .FindAsync(id, cancellationToken);

            if (shiftToDelete is null)
                return _logger.LogErrorAndReturnFail($"Shift with id = {id} is not in the database, nothing deleted");

            _context.Shifts.Remove(shiftToDelete);

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

    public async Task<Result<IReadOnlyList<ShiftResponse>>> GetAllShiftsAsync(CancellationToken cancellationToken = default)
    {

        try
        {
            var shifts = await _context.Shifts
                .AsNoTracking()
                .Include(shift => shift.Worker)
                .ToListAsync(cancellationToken);

            var shiftResponses = shifts
                .Select(shift => MapToShiftResponse(shift))
                .ToList();

            return Result<IReadOnlyList<ShiftResponse>>.Ok(shiftResponses);

        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<IReadOnlyList<ShiftResponse>>($"An unexpected error occurred loading shifts: {ex.Message}", ex);
        }

    }

    public async Task<Result<IReadOnlyList<ShiftResponse>>> GetAllShiftsByWorkerId(int workerId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (workerId <= 0)
                return _logger.LogErrorAndReturnFail<IReadOnlyList<ShiftResponse>>($"Worker id = {workerId} is invalid, worker ids must be greater than 0");

            var shifts = await _context.Shifts
                .AsNoTracking()
                .Where(shift => shift.WorkerId == workerId)
                .Include(shift => shift.Worker)
                .ToListAsync(cancellationToken);

            var shiftResponses = shifts
                .Select(shift => MapToShiftResponse(shift))
                .ToList();

            return Result<IReadOnlyList<ShiftResponse>>.Ok(shiftResponses);

        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<IReadOnlyList<ShiftResponse>>($"An unexpected error occurred loading shifts: {ex.Message}", ex);
        }

    }

    public async Task<Result<ShiftResponse>> GetShiftByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"Shift id = {id} is invalid, shift ids must be greater than 0");

            var shift = await _context.Shifts
                .Include(shift => shift.Worker)
                .FirstAsync(shift => shift.Id == id, cancellationToken);

            if (shift is null)
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"There is no shift available in the database with id = {id}");

            return Result<ShiftResponse>.Ok(MapToShiftResponse(shift));

        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<ShiftResponse>($"An unexpected error occurred loading shifts: {ex.Message}", ex);
        }

    }

    private static ShiftResponse MapToShiftResponse(Shift shift) => new()
    {
        Id = shift.Id,
        WorkerId = shift.WorkerId,
        WorkerName = shift.Worker.Name,
        WorkerDepartment = shift.Worker.Department,
        StartTime = shift.StartTime,
        EndTime = shift.EndTime,
        Duration = shift.EndTime - shift.StartTime,
    };

}
