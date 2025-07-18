using System.ComponentModel.DataAnnotations;

namespace ShiftLogger.API.DTOs;

public class UpdateShiftRequest
{
    [Required]
    public int WorkerId { get; set; }
    public string? WorkerName { get; set; }
    public string? WorkerDepartment { get; set; }
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }
}
