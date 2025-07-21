namespace ShiftLogger.Presentation.UI;

public interface IShiftLoggerUI
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
