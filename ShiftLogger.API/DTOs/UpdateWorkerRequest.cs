namespace ShiftLogger.API.DTOs;

public class UpdateWorkerRequest
{
    public string? Name { get; set; } = string.Empty;
    public string? Department { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? TelephoneNumber { get; set; }
}
