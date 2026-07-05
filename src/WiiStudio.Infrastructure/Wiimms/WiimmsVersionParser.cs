using System.Text.RegularExpressions;

namespace WiiStudio.Infrastructure.Wiimms;

internal static class WiimmsVersionParser
{
    private static readonly Regex VersionRegex = new(
        @"(?<major>\d+)\.(?<minor>\d+)(?:\.(?<patch>\d+))?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static Version Parse(string standardOutput)
    {
        ArgumentNullException.ThrowIfNull(standardOutput);

        var match = VersionRegex.Match(standardOutput);
        if (!match.Success)
        {
            throw new WiimmsVersionParseException(standardOutput);
        }

        var major = int.Parse(match.Groups["major"].Value);
        var minor = int.Parse(match.Groups["minor"].Value);
        if (!match.Groups["patch"].Success)
        {
            return new Version(major, minor);
        }

        var patch = int.Parse(match.Groups["patch"].Value);

        return new Version(major, minor, patch);
    }
}
