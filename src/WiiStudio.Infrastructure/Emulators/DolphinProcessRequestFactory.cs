using WiiStudio.Infrastructure.ExternalTools;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Emulators;

internal static class DolphinProcessRequestFactory
{
    public static ProcessRequest Create(
        string executablePath,
        IReadOnlyList<string> dolphinArguments,
        TimeSpan timeout,
        bool waitForExit,
        string? gameFilePath = null)
    {
        if (!OperatingSystem.IsLinux()
            || !string.Equals(
                Path.GetFileName(executablePath),
                ExternalToolDefinitionProvider.DolphinFlatpakApplicationId,
                StringComparison.Ordinal))
        {
            return new ProcessRequest(executablePath, dolphinArguments, timeout, waitForExit: waitForExit);
        }

        var arguments = new List<string> { "run" };
        var gameDirectory = string.IsNullOrWhiteSpace(gameFilePath)
            ? null
            : Path.GetDirectoryName(Path.GetFullPath(gameFilePath));
        if (!string.IsNullOrWhiteSpace(gameDirectory))
        {
            arguments.Add($"--filesystem={gameDirectory}:ro");
        }

        arguments.Add(ExternalToolDefinitionProvider.DolphinFlatpakApplicationId);
        arguments.AddRange(dolphinArguments);
        return new ProcessRequest(FindFlatpakExecutable(), arguments, timeout, waitForExit: waitForExit);
    }

    private static string FindFlatpakExecutable()
    {
        foreach (var candidate in new[] { "/usr/bin/flatpak", "/usr/local/bin/flatpak" })
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return "flatpak";
    }
}
