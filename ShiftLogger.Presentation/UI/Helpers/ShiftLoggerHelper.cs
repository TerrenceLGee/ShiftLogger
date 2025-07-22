using ShiftLogger.API.DTOs;
using ShiftLogger.API.Results;
using ShiftLogger.Presentation.Validation;
using System.Globalization;

namespace ShiftLogger.Presentation.UI.Helpers;

public static class ShiftLoggerHelper
{
    public static Result<DateTime> BuildDateTime(string dateString, string dateFormat)
    {
        CultureInfo info = CultureInfo.InvariantCulture;

        if (!ValidateInfo.IsValidDateString(dateString, dateFormat, info))
            return Result<DateTime>.Fail($"Invalid date, date must match format: {dateFormat}");

        var validTime =  DateTime.ParseExact(dateString, dateFormat, info);

        return Result<DateTime>.Ok(validTime);
    }

    public static Result<CreateWorkerRequest> BuildWorkerRequest(string name, string department, string? email, string? telephoneNumber)
    {
        if (!ValidateInfo.IsValidInputString(name))
            return Result<CreateWorkerRequest>.Fail("Worker name must be provided");

        if (!ValidateInfo.IsValidInputString(department))
            return Result<CreateWorkerRequest>.Fail("Worker department must be provided");
        
        var request =  new CreateWorkerRequest { Name = name, Department = department, Email = email, TelephoneNumber = telephoneNumber };

        return Result<CreateWorkerRequest>.Ok(request);
    }

    public static Result<UpdateWorkerRequest> BuildUpdateWorkerRequest(string? name, string? department, string? email, string? telephoneNumber)
    {
        if (new[] {name, department, email, telephoneNumber}
        .All(string.IsNullOrWhiteSpace))
            return Result<UpdateWorkerRequest>.Fail($"In order to update there must be at least one field provided");

        var request =  new UpdateWorkerRequest { Name = name, Department = department, Email = email, TelephoneNumber = telephoneNumber };

        return Result<UpdateWorkerRequest>.Ok(request);
    }

    public static Result<CreateShiftRequest> BuildCreateShiftRequest(int workerId, DateTime startTime, DateTime endTime)
    {
        if (!ValidateInfo.IsValidNumericInput(workerId))
            return Result<CreateShiftRequest>.Fail($"Worker id must be greater than 0");

        if (!ValidateInfo.IsValidEndTime(startTime, endTime))
            return Result<CreateShiftRequest>.Fail($"End time must come after start time");


        var request =  new CreateShiftRequest { WorkerId = workerId, StartTime = startTime, EndTime = endTime };

        return Result<CreateShiftRequest>.Ok(request);
    }

    public static Result<UpdateShiftRequest> BuildUpdateShiftRequest(int workerId, DateTime startTime, DateTime endTime)
    {
        if (!ValidateInfo.IsValidNumericInput(workerId))
            return Result<UpdateShiftRequest>.Fail("Worker id must be greater than 0");

        if (!ValidateInfo.IsValidEndTime(startTime, endTime))
            return Result<UpdateShiftRequest>.Fail("End time must come after start time");

        var request =  new UpdateShiftRequest { WorkerId = workerId, StartTime = startTime, EndTime = endTime };

        return Result<UpdateShiftRequest>.Ok(request);
    }

    public static bool IsValidWorkerId(IReadOnlyList<WorkerResponse> workers, int workerId)
    {
        if (!ValidateInfo.IsValidNumericInput(workerId))
        {
            ShiftLoggerUIHelper.DisplayMessage("Worker id must be greater than 0", "red");
            return false;
        }

        if (!workers.Any(worker => worker.WorkerId == workerId))
        {
            ShiftLoggerUIHelper.DisplayMessage($"There is no worker with id: {workerId} available", "red");
            return false;
        }
        return true;
    }

    public static bool IsValidShiftId(IReadOnlyList<ShiftResponse> shifts, int shiftId)
    {
        if (!ValidateInfo.IsValidNumericInput(shiftId))
        {
            ShiftLoggerUIHelper.DisplayMessage($"Shift id must be greater than 0", "red");
            return false;
        }

        if (!shifts.Any(shift => shift.Id == shiftId))
        {
            ShiftLoggerUIHelper.DisplayMessage($"There is no shift with id: {shiftId} available", "red");
            return false;
        }
        return true;
    }

    public static bool IsFailure<T>(Result<T> result, out T value)
    {
        if (!result.IsSuccess)
        {
            ShiftLoggerUIHelper.DisplayMessage(result.ErrorMessage, "red");
            value = default!;
            return true;
        }

        value = result.Value!;
        return false;
    }

    public static bool IsFailure(Result result)
    {
        if (!result.IsSuccess)
        {
            ShiftLoggerUIHelper.DisplayMessage(result.ErrorMessage);
            return true;
        }
        return false;
    }

    public static bool IsListEmpty<T>(IReadOnlyList<T> items, string itemType)
    {
        if (items.Count == 0)
        {
            ShiftLoggerUIHelper.DisplayMessage($"No {itemType} found", "yellow");
            return true;
        }
        return false;
    }
}

