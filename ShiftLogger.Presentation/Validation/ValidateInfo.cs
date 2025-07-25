using System.Globalization;

namespace ShiftLogger.Presentation.Validation;

public static class ValidateInfo
{
    public static bool IsValidInputString(string input)
    {
        return !string.IsNullOrWhiteSpace(input);
    }

    public static bool IsValidDateString(string dateString, string dateFormat, CultureInfo info)
    {
        return DateTime.TryParseExact(dateString, dateFormat, info, DateTimeStyles.None, out _);
    }

    public static bool IsValidEndTime(DateTime startTime, DateTime endTime)
    {
        return endTime > startTime;
    }

    public static bool IsValidNumericInput(int value)
    {
        return value > 0;
    }
}
