namespace ShiftLogger.API.Models;

public class Worker
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? TelephoneNumber { get; set; }
}
