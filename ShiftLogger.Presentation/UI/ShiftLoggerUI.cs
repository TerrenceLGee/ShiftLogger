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
    private const string DateFormat = "MM-dd-yyyy hh:mm";

    public ShiftLoggerUI(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var userChoice = DisplayMenuAndGetChoice();

            switch (userChoice)
            {
                case MenuOption.AddWorker:
                    await CreateWorkerAsync(cancellationToken);
                    break;
                case MenuOption.UpdateWorker:
                    await UpdateWorkerAsync(cancellationToken);
                    break;
                case MenuOption.DeleteWorker:
                    await DeleteWorkerAsync(cancellationToken);
                    break;
                case MenuOption.ShowWorkerById:
                    await ShowWorkerByIdAsync(cancellationToken);
                    break;
                case MenuOption.ShowWorkerByName:
                    await ShowWorkerByNameAsync(cancellationToken);
                    break;
                case MenuOption.ShowWorkers:
                    await ShowWorkersAsync(cancellationToken);
                    break;
                case MenuOption.CreateShift:
                    await CreateShiftAsync(cancellationToken);
                    break;
                case MenuOption.UpdateShift:
                    await UpdateShiftAsync(cancellationToken);
                    break;
                case MenuOption.DeleteShift:
                    await DeleteShiftAsync(cancellationToken);
                    break;
                case MenuOption.ShowShiftById:
                    await ShowShiftByIdAsync(cancellationToken);
                    break;
                case MenuOption.ShowShiftsByWorkerId:
                    await ShowShiftsByWorkerIdAsync(cancellationToken);
                    break;
                case MenuOption.ShowShifts:
                    await ShowShiftsAsync(cancellationToken);
                    break;
                case MenuOption.Exit:
                    return;
            }
        }
    }

    private async Task CreateWorkerAsync(CancellationToken cancellationToken = default)
    {
        var name = GetInput<string>("Please enter worker name: ").Trim();

        var department = GetInput<string>("Please enter worker's department: ").Trim();

        var email = GetOptionalInput($"Do you wish to enter an email for {name}? ")
            ? GetInput<string>("Please enter worker's email address: ").Trim()
            : null;

        var telephoneNumber = GetOptionalInput($"Do you wish to enter a telephone number for {name}? ")
            ? GetInput<string>("Please enter the worker's telephone number: ").Trim()
            : null;

        var createdWorkerResult = ShiftLoggerUIHelper.BuildWorkerRequest(name, department, email, telephoneNumber);


        if (!createdWorkerResult.IsSuccess)
        {
            DisplayMessage(createdWorkerResult.ErrorMessage, "red");
            return;
        }

        var createdWorker = createdWorkerResult.Value!;

        var creationResult = await ApiCallWithSpinnerAsync(
            $"Creating worker [yellow]{name}[/]...", cancellationToken =>
            _apiClient.CreateWorkerAsync(createdWorker, cancellationToken), cancellationToken);

        if (!creationResult.IsSuccess)
        {
            DisplayMessage(creationResult.ErrorMessage, "red");
            return;
        }

        DisplayMessage($"Successfully added worker with id = {creationResult.Value!.WorkerId}.", "green");
    }

    private async Task UpdateWorkerAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync(cancellationToken);

        var workerId = GetInput<int>("Enter the id for the worker to update: ");

        if (!ValidateInfo.IsValidNumericInput(workerId))
        {
            DisplayMessage("Worker id must be greater than 0", "red");
            return;
        }

        var name = GetOptionalInput("Do you wish to update the worker's name? ")
            ? GetInput<string>("Enter updated name: ").Trim()
            : null;

        var department = GetOptionalInput("Do you wish to update the worker's department? ")
            ? GetInput<string>("Enter updated department: ").Trim()
            : null;

        var email = GetOptionalInput("Do you wish to update the worker's email address? ")
            ? GetInput<string>("Enter updated email address: ").Trim()
            : null;

        var telephoneNumber = GetOptionalInput("Do you wish to update the worker's telephone number? ")
            ? GetInput<string>("Enter updated telephone number: ").Trim()
            : null;

        var updatedWorkerResult = ShiftLoggerUIHelper.BuildUpdateWorkerRequest(name, department, email, telephoneNumber);

        if (!updatedWorkerResult.IsSuccess)
        {
            DisplayMessage(updatedWorkerResult.ErrorMessage, "red");
            return;
        }

        var updatedWorker = updatedWorkerResult.Value!;

        var updateWorkerResult = await ApiCallWithSpinnerAsync($"Updating worker [yellow]{workerId}[/]...", cancellationToken => _apiClient.UpdateWorkerAsync(workerId, updatedWorker, cancellationToken), cancellationToken);

        if (!updatedWorkerResult.IsSuccess)
        {
            DisplayMessage(updatedWorkerResult.ErrorMessage, "red");
            return;
        }

        DisplayMessage($"Successfully updated worker {workerId}", "yellow");
    }

    private async Task DeleteWorkerAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync(cancellationToken);

        var workerId = GetInput<int>("Enter the id of the worker to delete: ");

        if (!ValidateInfo.IsValidNumericInput(workerId))
        {
            DisplayMessage("Worker id must be greater than 0", "red");
            return;
        }

        if (!GetOptionalInput($"Are you sure that you wish to delete worker [yellow]{workerId}[/]? "))
        {
            DisplayMessage($"Deletion canceled", "yellow");
            return;
        }

        var deletionResult = await ApiCallWithSpinnerAsync(
            $"Deleting worker [yellow]{workerId}[/]...", 
            cancellationToken => _apiClient.DeleteWorkerAsync(workerId, cancellationToken), cancellationToken);

        if (!deletionResult.IsSuccess)
        {
            DisplayMessage(deletionResult.ErrorMessage, "red");
            return;
        }

        DisplayMessage($"Successfully deleted worker with id = {workerId}.", "green");
    }

    private async Task ShowWorkerByIdAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync(cancellationToken);

        var workerId = GetInput<int>("Enter the id of the worker to see detailed information for: ");

        if (!ValidateInfo.IsValidNumericInput(workerId))
        {
            DisplayMessage("Worker id must be greater than 0", "red");
            return;
        }

        var workerRetrievalResult = await ApiCallWithSpinnerAsync(
            $"Retrieving information for worker [yellow]{workerId}[/]",
            cancellationToken => _apiClient.GetWorkerByIdAsync(workerId, cancellationToken), cancellationToken);

        if (!workerRetrievalResult.IsSuccess || workerRetrievalResult.Value is null)
        {
            DisplayMessage(workerRetrievalResult.ErrorMessage, "red");
            return;
        }

        DisplayWorker(workerRetrievalResult.Value);
    }

    private async Task ShowWorkerByNameAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync(cancellationToken);

        var name = GetInput<string>("Enter the name of the worker to see detailed information for: ");

        if (!ValidateInfo.IsValidInputString(name))
        {
            DisplayMessage("Invalid worker name", "red");
            return;
        }

        var retrievalResult = await ApiCallWithSpinnerAsync(
            $"Retrieving worker [yellow]{name}[/]...",
            cancellationToken => _apiClient.GetWorkersAsync(name, cancellationToken), cancellationToken);

        if (!retrievalResult.IsSuccess)
        {
            DisplayMessage(retrievalResult.ErrorMessage, "red");
            return;
        }

        DisplayWorkers(retrievalResult.Value!);
    }

    private async Task ShowWorkersAsync(CancellationToken cancellationToken = default)
    {
        var retrievalResult = await ApiCallWithSpinnerAsync(
            $"Retrieving workers...", 
            cancellationToken => _apiClient.GetWorkersAsync(null, cancellationToken), cancellationToken);


        if (!retrievalResult.IsSuccess)
        {
            DisplayMessage(retrievalResult.ErrorMessage, "red");
            return;
        }

        DisplayWorkers(retrievalResult.Value!);
    }

    private async Task CreateShiftAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync(cancellationToken);

        var workerId = GetInput<int>("Enter the id of the worker to log a shift for: ");

        var startTimeDateString = GetInput<string>($"Enter the start time in format: {DateFormat}: ");

        var endTimeDateString = GetInput<string>($"Enter the end time in format: {DateFormat}: ");

        var startTimeResult = ShiftLoggerUIHelper.BuildDateTime(startTimeDateString, DateFormat);

        if (!startTimeResult.IsSuccess)
        {
            DisplayMessage(startTimeResult.ErrorMessage, "red");
            return;
        }

        var endTimeResult = ShiftLoggerUIHelper.BuildDateTime(endTimeDateString, DateFormat);

        if (!endTimeResult.IsSuccess)
        {
            DisplayMessage(endTimeResult.ErrorMessage, "red");
            return;
        }

        var startTime = startTimeResult.Value!;
        var endTime = endTimeResult.Value!;

        var createShiftResult = ShiftLoggerUIHelper.BuildCreateShiftRequest(workerId, startTime, endTime);

        if (!createShiftResult.IsSuccess)
        {
            DisplayMessage(createShiftResult.ErrorMessage, "red");
            return;
        }

        var createdShift = createShiftResult.Value!;

        var creationResult = await ApiCallWithSpinnerAsync(
            $"Creating shift for worker [yellow]{workerId}[/]...", 
            cancellationToken => _apiClient.CreateShiftAsync(createdShift, cancellationToken), cancellationToken);

        if (!creationResult.IsSuccess)
        {
            DisplayMessage(creationResult.ErrorMessage, "red");
            return;
        }

        DisplayMessage($"Shift for worker with id = {workerId} added.", "green");
    }

    private async Task UpdateShiftAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync(cancellationToken);

        var workerId = GetInput<int>("Enter the id of the worker whose shift to update: ");

        if (!ValidateInfo.IsValidNumericInput(workerId))
        {
            DisplayMessage("Worker id must be greater than 0");
            return;
        }

        var startTimeDateString = GetInput<string>($"Enter the updated start time in format: {DateFormat}: ");

        var endTimeDateString = GetInput<string>($"Enter the updated end time in format: {DateFormat}: ");

        var startTimeResult = ShiftLoggerUIHelper.BuildDateTime(startTimeDateString, DateFormat);

        if (!startTimeResult.IsSuccess)
        {
            DisplayMessage(startTimeResult.ErrorMessage, "red");
            return;
        }

        var endTimeResult = ShiftLoggerUIHelper.BuildDateTime(endTimeDateString, DateFormat);

        if (!endTimeResult.IsSuccess)
        {
            DisplayMessage(endTimeResult.ErrorMessage, "red");
            return;
        }

        var startTime = startTimeResult.Value!;
        var endTime = endTimeResult.Value!;

        var updateShiftResult = ShiftLoggerUIHelper.BuildUpdateShiftRequest(workerId, startTime, endTime);

        if (!updateShiftResult.IsSuccess)
        {
            DisplayMessage(updateShiftResult.ErrorMessage, "red");
            return;
        }

        var updateShift = updateShiftResult.Value!;

        var updateResult = await ApiCallWithSpinnerAsync(
            $"Updating shift for worker [yellow]{workerId}[/]...",
            cancellationToken => _apiClient.UpdateShiftAsync(workerId, updateShift, cancellationToken), cancellationToken);

        if (!updateResult.IsSuccess)
        {
            DisplayMessage(updateResult.ErrorMessage, "red");
            return;
        }

        DisplayMessage($"Shift for worker with id = {workerId} updated.", "green");
    }

    private async Task DeleteShiftAsync(CancellationToken cancellationToken = default)
    {
        await ShowShiftsAsync(cancellationToken);

        var shiftId = GetInput<int>("Enter the id of the shift to delete: ");

        if (!ValidateInfo.IsValidNumericInput(shiftId))
        {
            DisplayMessage("Shift id must be greater than zero", "red");
            return;
        }

        if (!GetOptionalInput($"Are you sure you want to delete shift [yellow]{shiftId}[/]? "))
        {
            DisplayMessage("Deletion canceled", "yellow");
            return;
        }

        var deletionResult = await ApiCallWithSpinnerAsync(
            $"Delete shift [yellow]{shiftId}[/]...", 
            cancellationToken => _apiClient.DeleteShiftAsync(shiftId, cancellationToken), cancellationToken);

        if (!deletionResult.IsSuccess)
        {
            DisplayMessage(deletionResult.ErrorMessage, "red");
            return;
        }

        DisplayMessage($"Successfully delete shift with id = {shiftId}.");
    }

    private async Task ShowShiftByIdAsync(CancellationToken cancellationToken = default)
    {
        await ShowShiftsAsync(cancellationToken);

        var shiftId = GetInput<int>("Please enter the id of the shift that you wish to see detailed information for: ");

        if (!ValidateInfo.IsValidNumericInput(shiftId))
        {
            DisplayMessage("Shift id must be greater than zero", "red");
            return;
        }

        var retrievalResult = await ApiCallWithSpinnerAsync(
            $"Retrieving shifts with id [yellow]{shiftId}[/]...",
            cancellationToken => _apiClient.GetShiftByIdAsync(shiftId, cancellationToken), cancellationToken);

        if (!retrievalResult.IsSuccess)
        {
            DisplayMessage(retrievalResult.ErrorMessage, "red");
            return;
        }

        DisplayShift(retrievalResult.Value!);
    }

    private async Task ShowShiftsAsync(CancellationToken cancellationToken = default)
    {
        var retrievalResult = await ApiCallWithSpinnerAsync(
            $"Retrieving shifts...",
            cancellationToken => _apiClient.GetShiftsAsync(null, cancellationToken), cancellationToken);

        if (!retrievalResult.IsSuccess)
        {
            DisplayMessage(retrievalResult.ErrorMessage, "red");
            return;
        }

        DisplayShifts(retrievalResult.Value!);
    }

    private async Task ShowShiftsByWorkerIdAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync(cancellationToken);

        var workerId = GetInput<int>("Please enter the id for the worker who you wish to see shifts for: ");

        if (!ValidateInfo.IsValidNumericInput(workerId))
        {
            DisplayMessage("Worker id must be greater than 0");
            return;
        }

        var retrievalResult = await ApiCallWithSpinnerAsync(
            $"Retrieving shifts for worker [yellow]{workerId}[/]...",
            cancellationToken => _apiClient.GetShiftsAsync(workerId, cancellationToken), cancellationToken);

        if (!retrievalResult.IsSuccess)
        {
            DisplayMessage(retrievalResult.ErrorMessage, "red");
            return;
        }

        DisplayShifts(retrievalResult.Value!);
    }

    private T GetInput<T>(string message, string color = "teal")
    {
        return AnsiConsole.Ask<T>(message);
    }

    private void DisplayMessage(string? message, string color = "teal")
    {
        AnsiConsole.MarkupLine($"[{color}]{message}[/]");
    }

    private bool GetOptionalInput(string optional, string color = "teal")
    {
        return AnsiConsole.Confirm($"[{color}]{optional}[/]");
    }

    private void DisplayWorker(WorkerResponse worker)
    {
        DisplayMessage($"Information for worker {worker.WorkerId}:", "blue");
        DisplayMessage($"Name: {worker.Name}", "blue");
        DisplayMessage($"Department: {worker.Department}", "blue");
        DisplayMessage($"Email Address: {worker.Email ?? "[red]Not available[/]"}", "blue");
        DisplayMessage($"Telephone Number: {worker.TelephoneNumber ?? "[red]Not available[/]"}", "blue");
    }

    private void DisplayShift(ShiftResponse shift)
    {
        var durationInfo = $"{(int)shift.Duration.TotalHours} hours, {shift.Duration.TotalMinutes % 60} minutes";

        DisplayMessage($"Information for shift {shift.Id}", "blue");
        DisplayMessage($"Worker id: {shift.WorkerId}", "blue");
        DisplayMessage($"Worker name: {shift.WorkerName}", "blue");
        DisplayMessage($"Worker department: {shift.WorkerDepartment}", "blue");
        DisplayMessage($"Shift start time: {shift.StartTime.ToString(DateFormat)}", "blue");
        DisplayMessage($"Shift end time: {shift.EndTime.ToString(DateFormat)}", "blue");
        DisplayMessage($"Shift duration: {durationInfo}", "blue");
    }

    public void DisplayWorkers(IReadOnlyList<WorkerResponse> workers)
    {
        var table = new Table().Expand();
        table.AddColumn("Id");
        table.AddColumn("Name");
        table.AddColumn("Department");
        table.AddColumn("Email address");
        table.AddColumn("Telephone number");

        DisplayMessage("Workers\n", "underline blue");

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

    public void DisplayShifts(IReadOnlyList<ShiftResponse> shifts)
    {
        var table = new Table().Expand();
        table.AddColumn("Id");
        table.AddColumn("Worker Id");
        table.AddColumn("Worker Name");
        table.AddColumn("Worker Department");
        table.AddColumn("Shift Start Time");
        table.AddColumn("Shift End Time");
        table.AddColumn("Shift Duration");

        DisplayMessage("Shifts\n", "underline blue");

        foreach (var shift in shifts)
        {
            var durationInfo = $"{(int)shift.Duration.TotalHours} hours, {shift.Duration.TotalMinutes % 60} minutes";

            table.AddRow(
                shift.Id.ToString(),
                shift.WorkerId.ToString(),
                shift.WorkerName,
                shift.WorkerDepartment,
                shift.StartTime.ToString(DateFormat),
                shift.EndTime.ToString(DateFormat),
                durationInfo);
        }

        AnsiConsole.Write(table);
    }

    private async Task<Result> ApiCallWithSpinnerAsync(
        string label,
        Func<CancellationToken, Task<Result>> apiCall, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(label, ctx =>
                {
                    ctx.Status("Sending request...");
                    return apiCall(cancellationToken);
                });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
            return Result.Fail("Cancelled");
        }
    }

    private async Task<Result<T>> ApiCallWithSpinnerAsync<T>(
        string label,
        Func<CancellationToken, Task<Result<T>>> apiCall, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(label, ctx =>
                {
                    ctx.Status("Sending request...");
                    return apiCall(cancellationToken);
                });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
            return Result<T>.Fail("Cancelled");
        }
    }

    private MenuOption DisplayMenuAndGetChoice()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<MenuOption>()
            .Title("Please choose one of the following options:")
            .AddChoices(Enum.GetValues<MenuOption>())
            .UseConverter(choice => choice.GetDisplayName()));
    }
}

