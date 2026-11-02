using System.Globalization;
using System.Text.RegularExpressions;
using PawRescue.Mobile.Models;

namespace PawRescue.Mobile.Converters;

/// <summary>
/// POLYMORPHISM: every converter derives from this base and only overrides Convert.
/// The base handles the resource lookups and the unused ConvertBack.
/// </summary>
public abstract class OneWayConverter : IValueConverter
{
    public abstract object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    protected static Color Res(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var v) == true && v is Color c ? c : Colors.Gray;
}

public class SeverityToColorConverter : OneWayConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        SeverityLevel.Low => Res("SeverityLow"),
        SeverityLevel.Medium => Res("SeverityMedium"),
        SeverityLevel.High => Res("SeverityHigh"),
        SeverityLevel.Critical => Res("SeverityCritical"),
        _ => Colors.Gray
    };
}

public class StatusToColorConverter : OneWayConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        ReportStatus.Reported => Res("StatusReported"),
        ReportStatus.UnderReview => Res("StatusUnderReview"),
        ReportStatus.RescueAssigned => Res("StatusAssigned"),
        ReportStatus.InProgress => Res("StatusInProgress"),
        ReportStatus.Rescued => Res("StatusRescued"),
        _ => Res("StatusClosed")
    };
}

/// <summary>"RescueAssigned" → "Rescue Assigned". Works for any enum.</summary>
public class EnumToTextConverter : OneWayConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is null ? string.Empty : Regex.Replace(value.ToString()!, "(?<=[a-z])([A-Z])", " $1");
}

public class InvertedBoolConverter : OneWayConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;
}

public class IsNotNullConverter : OneWayConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s ? !string.IsNullOrWhiteSpace(s) : value is not null;
}

/// <summary>Picks TrueColor/FalseColor — e.g. timeline dots, unread notification background.</summary>
public class BoolToColorConverter : OneWayConverter
{
    public Color TrueColor { get; set; } = Colors.Orange;
    public Color FalseColor { get; set; } = Colors.LightGray;

    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? TrueColor : FalseColor;
}

public class UtcToLocalTextConverter : OneWayConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime dt) return string.Empty;
        var local = DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime();
        var format = parameter as string ?? "MMM d, h:mm tt";
        return local.ToString(format, culture);
    }
}

public class CountToBoolConverter : OneWayConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i > 0;
}
