using System.ComponentModel.DataAnnotations;

namespace ShiftLogger.API.DTOs;

public class CreateWorkerRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Department { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? TelephoneNumber { get; set; }
}
