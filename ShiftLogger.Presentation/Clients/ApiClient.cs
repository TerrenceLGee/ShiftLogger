using Microsoft.Extensions.Logging;
using ShiftLogger.API.DTOs;
using ShiftLogger.API.Extensions;
using ShiftLogger.API.Results;
using System.Text.Json;

namespace ShiftLogger.Presentation.Clients;

public class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiClient> _logger;
    private const string WorkersURL = "/api/workers";
    private const string ShiftsURL = "/api/shifts";

    public ApiClient(IHttpClientFactory httpClientFactory, ILogger<ApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    public async Task<Result<ShiftResponse>> CreateShiftAsync(CreateShiftRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var _httpClient = _httpClientFactory.CreateClient();

            var response = await _httpClient.GetAsync(WorkersURL, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return _logger.LogErrorAndReturnFail<ShiftResponse>($"API returned: {(int)response.StatusCode}: {error}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var apiResponse = JsonSerializer.Deserialize<ShiftResponse>(response)
        }
        
    }

    public Task<Result<WorkerResponse>> CreateWorkerAsync(CreateWorkerRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteShiftAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteWorkerAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ShiftResponse>> GetShiftByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyList<ShiftResponse>>> GetShiftsAsync(int? workerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<WorkerResponse>> GetWorkerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyList<WorkerResponse>>> GetWorkersAsync(string? nameFilter = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ShiftResponse>> UpdateShiftAsync(int id, UpdateShiftRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<WorkerResponse>> UpdateWorkerAsync(int id, UpdateWorkerRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

