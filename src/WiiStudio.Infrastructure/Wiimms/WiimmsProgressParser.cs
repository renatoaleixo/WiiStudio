using System.Globalization;
using System.Text.RegularExpressions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Wiimms;

public sealed class WiimmsProgressParser : IWiimmsProgressParser
{
    private static readonly Regex PercentRegex = new(
        @"(?<percent>\d{1,3}(?:[.,]\d+)?)\s*%",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ByteProgressRegex = new(
        @"(?<done>\d+(?:[.,]\d+)?)\s*(?<doneUnit>KiB|MiB|GiB|KB|MB|GB)\s*/\s*(?<total>\d+(?:[.,]\d+)?)\s*(?<totalUnit>KiB|MiB|GiB|KB|MB|GB)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex SpeedRegex = new(
        @"(?<speed>\d+(?:[.,]\d+)?)\s*(?<unit>KiB|MiB|GiB|KB|MB|GB)/s",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public bool TryParse(string line, out WiimmsOperationProgress progress)
    {
        progress = default!;
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var percentage = ParsePercentage(line);
        var (processed, total) = ParseByteProgress(line);
        var speed = ParseSpeed(line);
        var operation = ParseOperation(line);
        var hasUsefulData = percentage is not null || processed is not null || speed is not null || operation is not null;
        if (!hasUsefulData)
        {
            return false;
        }

        progress = new WiimmsOperationProgress(
            percentage,
            operation,
            ParseCurrentFile(line),
            processed,
            total,
            speed,
            EstimateRemaining(processed, total, speed),
            IsIndeterminate: percentage is null);
        return true;
    }

    private static double? ParsePercentage(string line)
    {
        var match = PercentRegex.Match(line);
        if (!match.Success)
        {
            return null;
        }

        var value = match.Groups["percent"].Value.Replace(',', '.');
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var percentage)
            ? Math.Clamp(percentage, 0, 100)
            : null;
    }

    private static (long? Processed, long? Total) ParseByteProgress(string line)
    {
        var match = ByteProgressRegex.Match(line);
        if (!match.Success)
        {
            return (null, null);
        }

        return (
            ParseBytes(match.Groups["done"].Value, match.Groups["doneUnit"].Value),
            ParseBytes(match.Groups["total"].Value, match.Groups["totalUnit"].Value));
    }

    private static double? ParseSpeed(string line)
    {
        var match = SpeedRegex.Match(line);
        if (!match.Success)
        {
            return null;
        }

        return ParseBytes(match.Groups["speed"].Value, match.Groups["unit"].Value);
    }

    private static long? ParseBytes(string value, string unit)
    {
        if (!double.TryParse(value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            return null;
        }

        var multiplier = unit.ToUpperInvariant() switch
        {
            "KIB" or "KB" => 1024d,
            "MIB" or "MB" => 1024d * 1024d,
            "GIB" or "GB" => 1024d * 1024d * 1024d,
            _ => 1d
        };

        return (long)(number * multiplier);
    }

    private static string? ParseOperation(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.Contains("copy", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("convert", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("add", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("remove", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("verify", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed.Length > 120 ? trimmed[..120] : trimmed;
        }

        return null;
    }

    private static string? ParseCurrentFile(string line)
    {
        var marker = line.LastIndexOf(" -> ", StringComparison.Ordinal);
        return marker >= 0 ? line[(marker + 4)..].Trim() : null;
    }

    private static TimeSpan? EstimateRemaining(long? processed, long? total, double? speed)
    {
        if (processed is null || total is null || speed is null || speed <= 0 || total <= processed)
        {
            return null;
        }

        return TimeSpan.FromSeconds((total.Value - processed.Value) / speed.Value);
    }
}
