using System.ComponentModel.DataAnnotations;

namespace ShiftLogger.Presentation.Menu.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        var fieldInfo = enumValue
            .GetType()
            .GetField(enumValue.ToString());

        if (fieldInfo?.GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] descriptionAttributes && descriptionAttributes.Length > 0)
            return descriptionAttributes[0].Name!;

        return enumValue.ToString();
    }
}

