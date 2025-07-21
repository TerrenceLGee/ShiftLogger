using Spectre.Console;
using ShiftLogger.Presentation.Clients;
using ShiftLogger.Presentation.UI.Helpers;
using ShiftLogger.API.Results;
using ShiftLogger.API.DTOs;

namespace ShiftLogger.Presentation.UI;

public class ShiftLoggerUI : IShiftLoggerUI
{
    private readonly IApiClient _apiClient;
    private const string DateFormat = "MM-dd-yyyy hh:mm";

    public ShiftLoggerUI(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"Creating worker [yellow]{name}[/]...", async ctx =>
            {
                ctx.Status("Sending request...");
                var creationResult = await _apiClient.CreateWorkerAsync(createdWorker, cancellationToken);

                if (!creationResult.IsSuccess)
                {
                    DisplayMessage(creationResult.ErrorMessage, "red");
                    return;
                }

                DisplayMessage($"Successfully added worker with id = {creationResult.Value!.WorkerId}.", "green");
            });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
        }
    }

    private async Task UpdateWorkerAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync();

        var workerId = GetInput<int>("Please enter the id for the worker that you wish to update: ");

        var name = GetOptionalInput("Do you wish to updated the worker's name? ")
            ? GetInput<string>("Please enter updated worker name: ")
            : null;

        var department = GetOptionalInput("Do you wish to update the worker's department? ")
            ? GetInput<string>("Please enter the updated worker department: ")
            : null;

        var email = GetOptionalInput("Do you wish to update the worker's email address? ")
            ? GetInput<string>("Please enter the updated worker email address: ")
            : null;

        var telephoneNumber = GetOptionalInput("Do you wish to update the worker's telephone number? ") 
            ? GetInput<string>("Please enter the updated worker telephone number: ")
            : null;

        var updatedWorkerResult = ShiftLoggerUIHelper.BuildUpdateWorkerRequest(name, department, email, telephoneNumber);

        if (!updatedWorkerResult.IsSuccess)
        {
            DisplayMessage(updatedWorkerResult.ErrorMessage, "red");
            return;
        }

        var updatedWorker = updatedWorkerResult.Value!;

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"Updating worker with id [yellow]{workerId}[/]...", async ctx =>
                {
                    ctx.Status("Sending request...");
                    var updateResult = await _apiClient.UpdateWorkerAsync(workerId, updatedWorker);

                    if (!updateResult.IsSuccess)
                    {
                        DisplayMessage(updateResult.ErrorMessage, "red");
                        return;
                    }

                    DisplayMessage($"Successfully updated worker with id = {workerId}.", "green");
                });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
        }
    }

    private async Task DeleteWorkerAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync();

        var workerId = GetInput<int>("Please enter the id of the worker that you wish to delete: ");

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"Deleting worker with id [yellow]{workerId}[/]...", async ctx =>
                {
                    ctx.Status("Sending request...");

                    var deletionResult = await _apiClient.DeleteWorkerAsync(workerId, cancellationToken);

                    if (!deletionResult.IsSuccess)
                    {
                        DisplayMessage(deletionResult.ErrorMessage, "red");
                        return;
                    }

                    DisplayMessage($"Successfully deleted worker with id = {workerId}.", "green");
                });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
        }
    }

    private async Task ShowWorkerByIdAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync();

        var workerId = GetInput<int>("Please enter the id of the worker you wish to see detailed information for: ");

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"Retrieving information for worker with id [yellow]{workerId}[/]", async ctx =>
                {
                    ctx.Status("Sending request...");
                    var workerRetrievalResult = await _apiClient.GetWorkerByIdAsync(workerId, cancellationToken)!;

                    if (!workerRetrievalResult.IsSuccess || workerRetrievalResult.Value is null)
                    {
                        DisplayMessage(workerRetrievalResult.ErrorMessage, "red");
                        return;
                    }

                    DisplayWorker(workerRetrievalResult.Value);
                });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
        }

    }

    private async Task ShowWorkerByNameAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync();

        var name = GetInput<string>("Please enter the name of the worker that you wish to see information for: ");

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"Retrieving workers...", async ctx =>
                {
                    ctx.Status("Sending request...");

                    var retrievalResult = await _apiClient.GetWorkersAsync(name, cancellationToken);

                    if (!retrievalResult.IsSuccess)
                    {
                        DisplayMessage(retrievalResult.ErrorMessage, "red");
                        return;
                    }

                    DisplayWorkers(retrievalResult.Value!);
                });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
        }


    }

    private async Task ShowWorkersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"Retrieving workers...", async ctx =>
            {
                ctx.Status("Sending request...");

                var retrievalResult = await _apiClient.GetWorkersAsync(null, cancellationToken);

                if (!retrievalResult.IsSuccess)
                {
                    DisplayMessage(retrievalResult.ErrorMessage, "red");
                    return;
                }

                DisplayWorkers(retrievalResult.Value!);
            });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
        }
    }

    private async Task CreateShiftAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync();

        var workerId = GetInput<int>("Please enter the id of the worker that you want to log a shift for: ");

        var startTimeDateString = GetInput<string>($"Please enter the start time in format: {DateFormat}: ");

        var endTimeDateString = GetInput<string>($"Please enter the end time in format: {DateFormat}: ");

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

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"Creating shift for worker [yellow]{workerId}[/]...", async ctx =>
                {
                    ctx.Status("Sending request...");
                    var creationResult = await _apiClient.CreateShiftAsync(createdShift, cancellationToken);

                    if (!creationResult.IsSuccess)
                    {
                        DisplayMessage(creationResult.ErrorMessage, "red");
                        return;
                    }

                    DisplayMessage($"Shift for worker with id = {workerId} added.", "green");
                });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
        }
    }

    private async Task UpdateShiftAsync(CancellationToken cancellationToken = default)
    {
        await ShowWorkersAsync();

        var workerId = GetInput<int>("Please enter the id of the worker, who's shift you wish to update: ");

        var startTimeDateString = GetInput<string>($"Please enter the start time in format: {DateFormat}: ");

        var endTimeDateString = GetInput<string>($"Please enter the end time in format: {DateFormat}: ");

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

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"Updating shift for worker [yellow]{workerId}[/]...", async ctx =>
                {
                    ctx.Status("Sending request...");
                    var updateResult = await _apiClient.UpdateShiftAsync(workerId, updateShift, cancellationToken);

                    if (!updateResult.IsSuccess)
                    {
                        DisplayMessage(updateResult.ErrorMessage, "red");
                        return;
                    }

                    DisplayMessage($"Shift for worker with id = {workerId} updated.", "green");
                });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
        }
    }

    private async Task DeleteShiftAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task ShowShiftByIdAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task ShowShiftsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task ShowShiftsByWorkerIdAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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
        var table = new Table();
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
        var table = new Table();
        table.AddColumn("Id");
        table.AddColumn("Worker Id");
        table.AddColumn("Worker Name");
        table.AddColumn("Worker Department");
        table.AddColumn("Shift Start Time");
        table.AddColumn("Shift End Time");
        table.AddColumn("Shift Duration");

        DisplayMessage("Shifts", "underline blue");

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
}

