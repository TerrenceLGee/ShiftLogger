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
    public Task<Result<ShiftResponse>> CreateShiftAsync(CreateShiftRequest request, CancellationToken cancellationToken = default) => SendAsync<ShiftResponse>(() => _httpClient.PostAsJsonAsync(ShiftsPath, request, cancellationToken), cancellationToken);


    public Task<Result<ShiftResponse>> UpdateShiftAsync(int id, UpdateShiftRequest request, CancellationToken cancellationToken = default) => SendAsync<ShiftResponse>(() => _httpClient.PutAsJsonAsync($"{ShiftsPath}/{id}", request, cancellationToken), cancellationToken);


    public Task<Result> DeleteShiftAsync(int id, CancellationToken cancellationToken = default) => DeleteAsync(() => _httpClient.DeleteAsync($"{ShiftsPath}/{id}", cancellationToken), cancellationToken);


    public Task<Result<ShiftResponse>> GetShiftByIdAsync(int id, CancellationToken cancellationToken = default) => SendGetAsync<ShiftResponse>(() => _httpClient.GetAsync($"{ShiftsPath}/{id}", cancellationToken), cancellationToken);


    public Task<Result<IReadOnlyList<ShiftResponse>>> GetShiftsAsync(int? workerId, CancellationToken cancellationToken = default) => SendGetAsync<IReadOnlyList<ShiftResponse>>(() => _httpClient.GetAsync(workerId is null ? ShiftsPath : $"{ShiftsPath}/worker/{workerId}", cancellationToken), cancellationToken);


    public Task<Result<WorkerResponse>> CreateWorkerAsync(CreateWorkerRequest request, CancellationToken cancellationToken = default) => SendAsync<WorkerResponse>(() => _httpClient.PostAsJsonAsync(WorkersPath, request, cancellationToken), cancellationToken);



    public Task<Result<WorkerResponse>> UpdateWorkerAsync(int id, UpdateWorkerRequest request, CancellationToken cancellationToken = default) => SendAsync<WorkerResponse>(() => _httpClient.PutAsJsonAsync($"{WorkersPath}/{id}", request, cancellationToken), cancellationToken);


    public Task<Result> DeleteWorkerAsync(int id, CancellationToken cancellationToken = default) => DeleteAsync(() => _httpClient.DeleteAsync($"{WorkersPath}/{id}", cancellationToken), cancellationToken);


    public Task<Result<WorkerResponse>> GetWorkerByIdAsync(int id, CancellationToken cancellationToken = default) => SendGetAsync<WorkerResponse>(() => _httpClient.GetAsync($"{WorkersPath}/{id}", cancellationToken), cancellationToken);


    public Task<Result<IReadOnlyList<WorkerResponse>>> GetWorkersAsync(string? nameFilter = null, CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(nameFilter)
            ? WorkersPath
            : QueryHelpers.AddQueryString(WorkersPath, "name", nameFilter!);

        return SendGetAsync<IReadOnlyList<WorkerResponse>>(() => _httpClient.GetAsync(url, cancellationToken), cancellationToken);
    }


    private async Task<Result<T>> SendAsync<T>(
        Func<Task<HttpResponseMessage>> send, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await send();

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return _logger.LogErrorAndReturnFail<T>($"API {(int)response.StatusCode}: {error}");
            }

            T? responseBody = await response.Content.ReadFromJsonAsync<T>(cancellationToken);

            return (responseBody is null)
                ? _logger.LogErrorAndReturnFail<T>($"Empty response body")
                : Result<T>.Ok(responseBody);
        }
        catch (HttpRequestException ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"Network error: {ex.Message} ");
        }
        catch (TaskCanceledException ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"Request timeout: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"Json parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"An unexpected error occurred: {ex.Message}");
        }
    }

    private async Task<Result> DeleteAsync(
        Func<Task<HttpResponseMessage>> send, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await send();

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return _logger.LogErrorAndReturnFail($"API {(int)response.StatusCode}: {error}");
            }

            return Result.Ok();
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
    }

    private async Task<Result<T>> SendGetAsync<T>(
        Func<Task<HttpResponseMessage>> get, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await get();

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return _logger.LogErrorAndReturnFail<T>($"API returned: {(int)response.StatusCode}: {error}");
            }

            var retrieved = await response.Content.ReadFromJsonAsync<T>(cancellationToken);

            return (retrieved is null)
                ? _logger.LogErrorAndReturnFail<T>("Empty response body")
                : Result<T>.Ok(retrieved);
        }
        catch (HttpRequestException ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"Network error: {ex.Message} ");
        }
        catch (TaskCanceledException ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"Request timeout: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"Json parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return _logger.LogErrorAndReturnFail<T>($"An unexpected error occurred: {ex.Message}");
        }
    }

}

