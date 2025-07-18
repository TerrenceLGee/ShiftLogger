using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftLogger.API.Models;

public class Shift
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public Worker Worker { get; set; } = new Worker();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    [NotMapped]
    public TimeSpan Duration => EndTime - StartTime;
}
