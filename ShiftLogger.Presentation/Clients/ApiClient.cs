using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ShiftLogger.API.DTOs;
using ShiftLogger.API.Extensions;
using ShiftLogger.API.Results;
using System.Net.Http.Json;
using System.Text.Json;

namespace ShiftLogger.Presentation.Clients;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private const string WorkersPath = "/api/workers";
    private const string ShiftsPath = "/api/shifts";

    public ApiClient(IHttpClientFactory httpClientFactory, ILogger<ApiClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Api");
        _logger = logger;
    }
    public Task<Result<ShiftResponse>> CreateShiftAsync(CreateShiftRequest request, CancellationToken cancellationToken = default) => SendAsync<ShiftResponse>(
        HttpMethod.Post, ShiftsPath, request, cancellationToken);


    public Task<Result<ShiftResponse>> UpdateShiftAsync(int id, UpdateShiftRequest request, CancellationToken cancellationToken = default) => SendAsync<ShiftResponse>(
        HttpMethod.Put, $"{ShiftsPath}/{id}", request, cancellationToken);


    public Task<Result> DeleteShiftAsync(int id, CancellationToken cancellationToken = default) => SendAsync(HttpMethod.Delete, $"{ShiftsPath}/{id}", payload: null, cancellationToken);


    public Task<Result<ShiftResponse>> GetShiftByIdAsync(int id, CancellationToken cancellationToken = default) => SendAsync<ShiftResponse>(
        HttpMethod.Get, $"{ShiftsPath}/{id}", payload: null, cancellationToken);


    public Task<Result<IReadOnlyList<ShiftResponse>>> GetShiftsAsync(int? workerId, CancellationToken cancellationToken = default)
    {
        var url = (workerId is null)
            ? ShiftsPath
            : $"{ShiftsPath}/worker/{workerId}";

        return SendAsync<IReadOnlyList<ShiftResponse>>(
            HttpMethod.Get, url, payload: null, cancellationToken);
    }


    public Task<Result<WorkerResponse>> CreateWorkerAsync(CreateWorkerRequest request, CancellationToken cancellationToken = default) => SendAsync<WorkerResponse>(
        HttpMethod.Post, WorkersPath, request, cancellationToken);



    public Task<Result<WorkerResponse>> UpdateWorkerAsync(int id, UpdateWorkerRequest request, CancellationToken cancellationToken = default) => SendAsync<WorkerResponse>(
        HttpMethod.Put, $"{WorkersPath}/{id}", request, cancellationToken);


    public Task<Result> DeleteWorkerAsync(int id, CancellationToken cancellationToken = default) => SendAsync(HttpMethod.Delete, $"{WorkersPath}/{id}", payload: null,  cancellationToken);


    public Task<Result<WorkerResponse>> GetWorkerByIdAsync(int id, CancellationToken cancellationToken = default) => SendAsync<WorkerResponse>(
        HttpMethod.Get, $"{WorkersPath}/{id}", payload: null, cancellationToken);


    public Task<Result<IReadOnlyList<WorkerResponse>>> GetWorkersAsync(string? nameFilter = null, CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(nameFilter)
            ? WorkersPath
            : QueryHelpers.AddQueryString(WorkersPath, "name", nameFilter!);

        return SendAsync<IReadOnlyList<WorkerResponse>>(
            HttpMethod.Get, url, payload: null, cancellationToken);
    }


    private async Task<Result<T>> SendAsync<T>(
        HttpMethod method,
        string url,
        object? payload = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, url);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"Network error: {ex.Message} ");
        }
        catch (TaskCanceledException ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"Request timeout: {ex.Message}");
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"An unexpected error occurred: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return _logger.LogErrorAndReturnFail<T>($"API returned: {(int)response.StatusCode}: {error}");
        }

        T? responseBody;
        try
        {
            responseBody = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        }
        catch (JsonException ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"JSON parse error: {ex.Message}");
        }

        return (responseBody is null)
            ? _logger.LogErrorAndReturnFail<T>("Empty response body")
            : Result<T>.Ok(responseBody);
    }

    private async Task<Result> SendAsync(
        HttpMethod method,
        string url,
        object? payload = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, url);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return _logger.LogErrorAndReturnFail($"Network error: {ex.Message} ");
        }
        catch (TaskCanceledException ex)
        {
            return _logger.LogErrorAndReturnFail($"Request timeout: {ex.Message}");
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail($"An unexpected error occurred: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return _logger.LogErrorAndReturnFail($"API returned: {(int)response.StatusCode}: {error}");
        }
        return Result.Ok();
    }
}

