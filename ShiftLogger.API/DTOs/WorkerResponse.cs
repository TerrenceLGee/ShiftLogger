namespace ShiftLogger.API.DTOs;

public class WorkerResponse
{
    public int WorkerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? TelephoneNumber { get; set; }
}
