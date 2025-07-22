using ShiftLogger.API.Results;
using Spectre.Console;

namespace ShiftLogger.Presentation.UI.Helpers;

public static class ShiftLoggerUIHelper
{
    public static async Task<Result<T>> ApiCallWithSpinnerAsync<T>(
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
                    ctx.Status("[yellow]Sending request...[/]");
                    return apiCall(cancellationToken);
                });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
            return Result<T>.Fail("Cancelled");
        }
    }

    public static async Task<Result> ApiCallWithSpinnerAsync(
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
                    ctx.Status("[yellow]Sending request...[/]");
                    return apiCall(cancellationToken);
                });
        }
        catch (OperationCanceledException)
        {
            DisplayMessage("Operation cancelled by user", "yellow");
            return Result.Fail("Cancelled");
        }
    }

    public static void ShowPaginatedItems<T>(IReadOnlyList<T> items, string entityName, Action<IReadOnlyList<T>> display, int pageSize = 10)
    {
        if (ShiftLoggerHelper.IsListEmpty(items, entityName))
            return;

        int pageIndex = 0;
        int pageCount = (int)Math.Ceiling(items.Count / (double)pageSize);

        while (true)
        {
            var pageItems = items
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();

            DisplayMessage(
                $"Page {pageIndex + 1} of {pageCount} (showing {pageItems.Count} of {items.Count})", "blue");

            display(pageItems);

            var prompt = new SelectionPrompt<string>()
                .Title("Navigate pages:");

            if (pageIndex > 0)
                prompt.AddChoice("Previous");

            prompt.AddChoice("Exit");

            if (pageIndex < pageCount - 1)
                prompt.AddChoice("Next");

            var choice = AnsiConsole.Prompt(prompt);

            if (choice == "Next" && pageIndex < pageCount - 1)
                pageIndex++;
            else if (choice == "Previous" && pageIndex > 0)
                pageIndex--;
            else break;
        }
    }

    public static T GetInput<T>(string message, string color = "teal")
    {
        return AnsiConsole.Ask<T>(message);
    }

    public static void DisplayMessage(string? message, string color = "teal")
    {
        AnsiConsole.MarkupLine($"[{color}]{message}[/]");
    }

    public static bool GetOptionalInput(string optional, string color = "teal")
    {
        return AnsiConsole.Confirm($"[{color}]{optional}[/]");
    }

    public static string PromptUpdateDate(string label, string dateFormat, DateTime current)
    {
        return GetOptionalInput($"Do you wish to update the {label} time? ")
            ? GetInput<string>($"Enter updated {label} time (24-hour clock 0-23) in format: ({dateFormat})").Trim()
            : current.ToString(dateFormat);
    }
}

