using Microsoft.OpenApi.Extensions;
using ShiftLogger.API.DTOs;
using ShiftLogger.API.Results;
using ShiftLogger.Presentation.Clients;
using ShiftLogger.Presentation.Menu;
using ShiftLogger.Presentation.UI.Helpers;
using ShiftLogger.Presentation.Validation;
using Spectre.Console;

namespace ShiftLogger.Presentation.UI;

public class ShiftLoggerUI : IShiftLoggerUI
{
    private readonly IApiClient _apiClient;
    private const string DateFormat = "MM-dd-yyyy HH:mm";
    private const string DurationFormat = @"hh\:mm";
    private readonly IReadOnlyDictionary<MenuOption, Func<CancellationToken, Task>> _handlers;

    public ShiftLoggerUI(IApiClient apiClient)
    {
        _apiClient = apiClient;
        _handlers = new Dictionary<MenuOption, Func<CancellationToken, Task>>
        {
            [MenuOption.AddWorker] = CreateWorkerAsync,
            [MenuOption.UpdateWorker] = UpdateWorkerAsync,
            [MenuOption.DeleteWorker] = DeleteWorkerAsync,
            [MenuOption.ShowWorkerById] = ShowWorkerByIdAsync,
            [MenuOption.ShowWorkerByName] = ShowWorkerByNameAsync,
            [MenuOption.ShowWorkers] = ShowWorkersAsync,
            [MenuOption.CreateShift] = CreateShiftAsync,
            [MenuOption.UpdateShift] = UpdateShiftAsync,
            [MenuOption.DeleteShift] = DeleteShiftAsync,
            [MenuOption.ShowShiftById] = ShowShiftByIdAsync,
            [MenuOption.ShowShiftsByWorkerId] = ShowShiftsByWorkerIdAsync,
            [MenuOption.ShowShifts] = ShowShiftsAsync,
            [MenuOption.Exit] = _ => Task.FromResult(0),
        };
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            MenuOption userChoice;

            try
            {
                userChoice = DisplayMenuAndGetChoice();
            }
            catch (OperationCanceledException)
            {
                ShiftLoggerUIHelper.DisplayMessage("Operation cancelled. Exiting.", "yellow");
                return;
            }

            if (!_handlers.TryGetValue(userChoice, out var handler))
            {
                ShiftLoggerUIHelper.DisplayMessage($"No handler for {userChoice}", "red");
                continue;
            }

            await handler(cancellationToken);

            if (userChoice == MenuOption.Exit)
            {
                ShiftLoggerUIHelper.DisplayMessage("Goodbye", "green");
                break;
            }
            Pause();
        }
    }

    private async Task CreateWorkerAsync(CancellationToken cancellationToken = default)
    {
        var name = ShiftLoggerUIHelper.GetInput<string>("Enter worker name: ").Trim();

        var department = ShiftLoggerUIHelper.GetInput<string>("Enter worker's department: ").Trim();

        var email = ShiftLoggerUIHelper.GetOptionalInput($"Do you wish to enter an email for {name}? ")
            ? ShiftLoggerUIHelper.GetInput<string>("Enter worker's email address: ").Trim()
            : null;

        var telephoneNumber = ShiftLoggerUIHelper.GetOptionalInput($"Do you wish to enter a telephone number for {name}? ")
            ? ShiftLoggerUIHelper.GetInput<string>("Enter the worker's telephone number: ").Trim()
            : null;

        var createdWorkerResult = ShiftLoggerHelper.BuildWorkerRequest(name, department, email, telephoneNumber);


        if (ShiftLoggerHelper.IsFailure(createdWorkerResult, out var createdWorker))
            return;

        if (!ShiftLoggerUIHelper.GetOptionalInput("Confirm adding worker? "))
        {
            ShiftLoggerUIHelper.DisplayMessage("Worker addition cancelled", "yellow");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var creationResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync(
            $"Creating worker {name}...", cancellationToken =>
            _apiClient.CreateWorkerAsync(createdWorker, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(creationResult, out var created))
            return;

        ShiftLoggerUIHelper.DisplayMessage($"Successfully added worker with id = {created.WorkerId}.", "green");
    }

    private async Task UpdateWorkerAsync(CancellationToken cancellationToken = default)
    {
        var workersResult = await _apiClient.GetWorkersAsync(null, cancellationToken);

        if (ShiftLoggerHelper.IsFailure(workersResult, out var workers))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(workers, "workers", DisplayWorkers);

        var workerId = ShiftLoggerUIHelper.GetInput<int>("Enter the id for the worker to update: ");

        if (!ShiftLoggerHelper.IsValidWorkerId(workers, workerId))
            return;

        var name = ShiftLoggerUIHelper.GetOptionalInput("Do you wish to update the worker's name? ")
            ? ShiftLoggerUIHelper.GetInput<string>("Enter updated name: ").Trim()
            : null;

        var department = ShiftLoggerUIHelper.GetOptionalInput("Do you wish to update the worker's department? ")
            ? ShiftLoggerUIHelper.GetInput<string>("Enter updated department: ").Trim()
            : null;

        var email = ShiftLoggerUIHelper.GetOptionalInput("Do you wish to update the worker's email address? ")
            ? ShiftLoggerUIHelper.GetInput<string>("Enter updated email address: ").Trim()
            : null;

        var telephoneNumber = ShiftLoggerUIHelper.GetOptionalInput("Do you wish to update the worker's telephone number? ")
            ? ShiftLoggerUIHelper.GetInput<string>("Enter updated telephone number: ").Trim()
            : null;

        var updatedWorkerResult = ShiftLoggerHelper.BuildUpdateWorkerRequest(name, department, email, telephoneNumber);

        if (ShiftLoggerHelper.IsFailure(updatedWorkerResult, out var updatedWorker))
            return;

        cancellationToken.ThrowIfCancellationRequested();

        var updateWorkerResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync($"Updating worker {workerId}...", cancellationToken => _apiClient.UpdateWorkerAsync(workerId, updatedWorker, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(updateWorkerResult))
            return;

        ShiftLoggerUIHelper.DisplayMessage($"Successfully updated worker {workerId}", "yellow");
    }

    private async Task DeleteWorkerAsync(CancellationToken cancellationToken = default)
    {
        var workersResult = await _apiClient.GetWorkersAsync();

        if (ShiftLoggerHelper.IsFailure(workersResult, out var workers))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(workers, "workers", DisplayWorkers);

        var workerId = ShiftLoggerUIHelper.GetInput<int>("Enter the id of the worker to delete: ");

        if (!ShiftLoggerHelper.IsValidWorkerId(workers, workerId))
            return;

        if (!ShiftLoggerUIHelper.GetOptionalInput($"Are you sure that you wish to delete worker [yellow]{workerId}[/]? "))
        {
            ShiftLoggerUIHelper.DisplayMessage($"Deletion canceled", "yellow");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var deletionResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync(
            $"Deleting worker {workerId}...",
            cancellationToken => _apiClient.DeleteWorkerAsync(workerId, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(deletionResult))
            return;

        ShiftLoggerUIHelper.DisplayMessage($"Successfully deleted worker with id = {workerId}.", "green");
    }

    private async Task ShowWorkerByIdAsync(CancellationToken cancellationToken = default)
    {
        var workersResult = await _apiClient.GetWorkersAsync();

        if (ShiftLoggerHelper.IsFailure(workersResult, out var workers))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(workers, "workers", DisplayWorkers);

        var workerId = ShiftLoggerUIHelper.GetInput<int>("Enter worker id to see details: ");

        if (!ShiftLoggerHelper.IsValidWorkerId(workers, workerId))
            return;

        cancellationToken.ThrowIfCancellationRequested();

        var workerRetrievalResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync(
            $"Retrieving information for worker {workerId}",
            cancellationToken => _apiClient.GetWorkerByIdAsync(workerId, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(workerRetrievalResult, out var retrieved))
            return;

        DisplayWorker(retrieved);
    }

    private async Task ShowWorkerByNameAsync(CancellationToken cancellationToken = default)
    {
        var workersResult = await _apiClient.GetWorkersAsync();

        if (ShiftLoggerHelper.IsFailure(workersResult, out var workers))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(workers, "workers", DisplayWorkers);

        var name = ShiftLoggerUIHelper.GetInput<string>("Enter worker name to see details: ").Trim();

        if (!ValidateInfo.IsValidInputString(name))
        {
            ShiftLoggerUIHelper.DisplayMessage("Invalid worker name", "red");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var retrievalResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync(
            $"Retrieving worker {name}...",
            cancellationToken => _apiClient.GetWorkersAsync(name, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(retrievalResult, out var retrieved))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(retrieved, $"worker matching the name {name}", DisplayWorkers);
    }

    private async Task ShowWorkersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var retrievalResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync(
            $"Retrieving workers...",
            cancellationToken => _apiClient.GetWorkersAsync(null, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(retrievalResult, out var retrieved))
            return;

        if (ShiftLoggerHelper.IsListEmpty(retrieved, "workers"))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(retrieved, "workers", DisplayWorkers);
    }

    private async Task CreateShiftAsync(CancellationToken cancellationToken = default)
    {
        var workerResult = await _apiClient.GetWorkersAsync();

        if (ShiftLoggerHelper.IsFailure(workerResult, out var workers))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(workers, "workers", DisplayWorkers);

        var workerId = ShiftLoggerUIHelper.GetInput<int>("Enter worker id to log shift: ");

        if (!ShiftLoggerHelper.IsValidWorkerId(workers, workerId))
            return;

        var startTimeDateString = ShiftLoggerUIHelper.GetInput<string>($"Enter the start time (24-hour clock 0-23) in format: ({DateFormat}): ").Trim();

        var endTimeDateString = ShiftLoggerUIHelper.GetInput<string>($"Enter the end time (24-hour clock 0-23) in format: ({DateFormat}): ").Trim();

        var startTimeResult = ShiftLoggerHelper.BuildDateTime(startTimeDateString, DateFormat);

        if (ShiftLoggerHelper.IsFailure(startTimeResult))
            return;

        var endTimeResult = ShiftLoggerHelper.BuildDateTime(endTimeDateString, DateFormat);

        if (ShiftLoggerHelper.IsFailure(endTimeResult))
            return;

        var startTime = startTimeResult.Value!;
        var endTime = endTimeResult.Value!;

        var createShiftResult = ShiftLoggerHelper.BuildCreateShiftRequest(workerId, startTime, endTime);

        if (ShiftLoggerHelper.IsFailure(createShiftResult, out var createdShift))
            return;

        if (!ShiftLoggerUIHelper.GetOptionalInput("Confirm logging shift? "))
        {
            ShiftLoggerUIHelper.DisplayMessage("Shift logging cancelled", "yellow");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var creationResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync(
            $"Creating shift for worker {workerId}...",
            cancellationToken => _apiClient.CreateShiftAsync(createdShift, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(creationResult, out var created))
            return;

        ShiftLoggerUIHelper.DisplayMessage($"Shift for worker with id = {created.WorkerId} added.", "green");
    }

    private async Task UpdateShiftAsync(CancellationToken cancellationToken = default)
    {
        var workerResult = await _apiClient.GetWorkersAsync();

        if (ShiftLoggerHelper.IsFailure(workerResult, out var workers))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(workers, "workers", DisplayWorkers);

        var workerId = ShiftLoggerUIHelper.GetInput<int>("Enter worker id to update shift: ");

        if (!ShiftLoggerHelper.IsValidWorkerId(workers, workerId))
            return;

        var shiftsForWorkerIdResult = await _apiClient.GetShiftsAsync(workerId, cancellationToken);

        if (ShiftLoggerHelper.IsFailure(shiftsForWorkerIdResult, out var shiftsForWorkerId))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(shiftsForWorkerId, $"shifts for worker {workerId}", DisplayShifts);

        var shiftId = ShiftLoggerUIHelper.GetInput<int>("Enter the id of the shift to update: ");

        if (!ShiftLoggerHelper.IsValidShiftId(shiftsForWorkerId, shiftId))
            return;

        var shiftResult = await _apiClient.GetShiftByIdAsync(shiftId, cancellationToken);

        if (ShiftLoggerHelper.IsFailure(shiftResult, out var shift))
            return;

        var startTimeDateString = ShiftLoggerUIHelper.PromptUpdateDate("start", DateFormat,  shift.StartTime);

        var endTimeDateString = ShiftLoggerUIHelper.PromptUpdateDate("end", DateFormat, shift.EndTime);

        var startTimeResult = ShiftLoggerHelper.BuildDateTime(startTimeDateString, DateFormat);

        if (ShiftLoggerHelper.IsFailure(startTimeResult, out var startTime))
            return;

        var endTimeResult = ShiftLoggerHelper.BuildDateTime(endTimeDateString, DateFormat);

        if (ShiftLoggerHelper.IsFailure(endTimeResult, out var endTime))
            return;

        var updateShiftResult = ShiftLoggerHelper.BuildUpdateShiftRequest(workerId, startTime, endTime);

        if (ShiftLoggerHelper.IsFailure(updateShiftResult, out var updateShift))
            return;

        if (!ShiftLoggerUIHelper.GetOptionalInput($"Confirm updating shift# {shiftId} for worker {workerId}? "))
        {
            ShiftLoggerUIHelper.DisplayMessage("Shift logging update cancelled", "yellow");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var updateResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync(
            $"Updating shift for worker {workerId}...",
            cancellationToken => _apiClient.UpdateShiftAsync(shiftId, updateShift, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(updateResult, out var updatedShift))
            return;

        ShiftLoggerUIHelper.DisplayMessage($"Shift# {shiftId} for worker {updatedShift.WorkerId} updated.", "green");
    }

    private async Task DeleteShiftAsync(CancellationToken cancellationToken = default)
    {
        var shiftResult = await _apiClient.GetShiftsAsync(null, cancellationToken);

        if (ShiftLoggerHelper.IsFailure(shiftResult, out var shifts))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(shifts, "shifts", DisplayShifts);

        var shiftId = ShiftLoggerUIHelper.GetInput<int>("Enter the id of the shift to delete: ");

        if (!ShiftLoggerHelper.IsValidShiftId(shifts, shiftId))
            return;

        if (!ShiftLoggerUIHelper.GetOptionalInput($"Are you sure you want to delete shift [yellow]{shiftId}[/]? "))
        {
            ShiftLoggerUIHelper.DisplayMessage("Deletion canceled", "yellow");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var deletionResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync(
            $"Deleting shift {shiftId}...",
            cancellationToken => _apiClient.DeleteShiftAsync(shiftId, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(deletionResult))
            return;

        ShiftLoggerUIHelper.DisplayMessage($"Successfully deleted shift with id = {shiftId}.", "green");
    }

    private async Task ShowShiftByIdAsync(CancellationToken cancellationToken = default)
    {
        var shiftResult = await _apiClient.GetShiftsAsync(null, cancellationToken);

        if (ShiftLoggerHelper.IsFailure(shiftResult, out var shifts))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(shifts, "shifts", DisplayShifts);

        var shiftId = ShiftLoggerUIHelper.GetInput<int>("Enter shift id to view details: ");

        if (!ShiftLoggerHelper.IsValidShiftId(shifts, shiftId))
            return;

        cancellationToken.ThrowIfCancellationRequested();

        var retrievalResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync(
            $"Retrieving shift {shiftId}...",
            cancellationToken => _apiClient.GetShiftByIdAsync(shiftId, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(retrievalResult, out var retrieved))
            return;

        DisplayShift(retrieved);
    }

    private async Task ShowShiftsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var retrievalResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync(
            "Retrieving shifts...",
            cancellationToken => _apiClient.GetShiftsAsync(null, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(retrievalResult, out var retrieved))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(retrieved, "shifts", DisplayShifts);
    }

    private async Task ShowShiftsByWorkerIdAsync(CancellationToken cancellationToken = default)
    {
        var workersResult = await _apiClient.GetWorkersAsync();

        if (ShiftLoggerHelper.IsFailure(workersResult, out var workers))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(workers, "workers", DisplayWorkers);

        var workerId = ShiftLoggerUIHelper.GetInput<int>("Enter the worker id to view shifts: ");

        if (!ShiftLoggerHelper.IsValidWorkerId(workers, workerId))
            return;

        cancellationToken.ThrowIfCancellationRequested();

        var retrievalResult = await ShiftLoggerUIHelper.ApiCallWithSpinnerAsync(
            $"Retrieving shifts for worker {workerId}...",
            cancellationToken => _apiClient.GetShiftsAsync(workerId, cancellationToken), cancellationToken);

        if (ShiftLoggerHelper.IsFailure(retrievalResult, out var retrieved))
            return;

        ShiftLoggerUIHelper.ShowPaginatedItems(retrieved, $"shifts for worker {workerId}", DisplayShifts);
    }

    

    private void DisplayWorker(WorkerResponse worker)
    {
        ShiftLoggerUIHelper.DisplayMessage($"Information for worker {worker.WorkerId}:", "blue");
        ShiftLoggerUIHelper.DisplayMessage($"Name: {worker.Name}", "blue");
        ShiftLoggerUIHelper.DisplayMessage($"Department: {worker.Department}", "blue");
        ShiftLoggerUIHelper.DisplayMessage($"Email Address: {worker.Email ?? "[red]Not available[/]"}", "blue");
        ShiftLoggerUIHelper.DisplayMessage($"Telephone Number: {worker.TelephoneNumber ?? "[red]Not available[/]"}", "blue");
    }

    private void DisplayShift(ShiftResponse shift)
    {
        ShiftLoggerUIHelper.DisplayMessage($"Information for shift {shift.Id}", "blue");
        ShiftLoggerUIHelper.DisplayMessage($"Worker id: {shift.WorkerId}", "blue");
        ShiftLoggerUIHelper.DisplayMessage($"Worker name: {shift.WorkerName}", "blue");
        ShiftLoggerUIHelper.DisplayMessage($"Worker department: {shift.WorkerDepartment}", "blue");
        ShiftLoggerUIHelper.DisplayMessage($"Shift start time: {shift.StartTime.ToString(DateFormat)}", "blue");
        ShiftLoggerUIHelper.DisplayMessage($"Shift end time: {shift.EndTime.ToString(DateFormat)}", "blue");
        ShiftLoggerUIHelper.DisplayMessage($"Shift duration: {shift.Duration.ToString(DurationFormat)}", "blue");
    }

    private void DisplayWorkers(IReadOnlyList<WorkerResponse> workers)
    {
        var table = new Table().Expand();
        table.AddColumn("Id");
        table.AddColumn("Name");
        table.AddColumn("Department");
        table.AddColumn("Email address");
        table.AddColumn("Telephone number");

        ShiftLoggerUIHelper.DisplayMessage("Workers\n", "underline blue");

        foreach (var worker in workers)
        {
            table.AddRow(
                worker.WorkerId.ToString(),
                worker.Name,
                worker.Department,
                worker.Email ?? "Not available",
                worker.TelephoneNumber ?? "Not available");
        }

        AnsiConsole.Write(table);
    }

    private void DisplayShifts(IReadOnlyList<ShiftResponse> shifts)
    {
        var table = new Table().Expand();
        table.AddColumn("Id");
        table.AddColumn("Worker Id");
        table.AddColumn("Worker Name");
        table.AddColumn("Worker Department");
        table.AddColumn("Shift Start Time");
        table.AddColumn("Shift End Time");
        table.AddColumn("Shift Duration");

        ShiftLoggerUIHelper.DisplayMessage("Shifts\n", "underline blue");

        foreach (var shift in shifts)
        {
            table.AddRow(
                shift.Id.ToString(),
                shift.WorkerId.ToString(),
                shift.WorkerName,
                shift.WorkerDepartment,
                shift.StartTime.ToString(DateFormat),
                shift.EndTime.ToString(DateFormat),
                shift.Duration.ToString(DurationFormat));
        }

        AnsiConsole.Write(table);
    }

    
    private MenuOption DisplayMenuAndGetChoice()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<MenuOption>()
            .Title("Please choose one of the following options:")
            .AddChoices(Enum.GetValues<MenuOption>())
            .UseConverter(choice => choice.GetDisplayName()));
    }


    private void Pause()
    {
        ShiftLoggerUIHelper.DisplayMessage("Press any key to return to the main menu", "grey");
        Console.ReadKey();
        AnsiConsole.Clear();
    }
}

