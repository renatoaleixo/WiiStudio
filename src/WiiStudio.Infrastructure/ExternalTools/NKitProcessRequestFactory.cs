using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.ExternalTools;

internal static class NKitProcessRequestFactory
{
    public static bool IsRuntimeAvailable() => OperatingSystem.IsWindows() || FindMonoExecutable() is not null;

    public static ProcessRequest Create(
        string nkitExecutablePath,
        IReadOnlyList<string> arguments,
        TimeSpan timeout)
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest(nkitExecutablePath, arguments, timeout);
        }

        var mono = FindMonoExecutable()
            ?? throw new InvalidOperationException("O runtime Mono é necessário para executar o NKit Legacy neste sistema.");
        return new ProcessRequest(mono, [nkitExecutablePath, .. arguments], timeout);
    }

    private static string? FindMonoExecutable()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(path))
        {
            foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                var candidate = Path.Combine(directory, OperatingSystem.IsWindows() ? "mono.exe" : "mono");
                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }
            }
        }

        return null;
    }
}
