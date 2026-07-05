namespace WiiStudio.Infrastructure.Wiimms;

public sealed class WiimmsToolLocator : IWiimmsToolLocator
{
    private readonly WiimmsToolOptions _options;

    public WiimmsToolLocator(WiimmsToolOptions options)
    {
        _options = options;
    }

    public Task<string> FindWitAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(FindTool("wit", _options.WitExecutablePath));
    }

    public Task<string> FindWwtAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(FindTool("wwt", _options.WwtExecutablePath));
    }

    private string FindTool(string toolName, string? configuredPath)
    {
        return FindConfiguredPath(configuredPath)
            ?? FindInPath(toolName)
            ?? FindInDirectory(_options.ToolsDirectory, toolName)
            ?? FindInDirectory(_options.ApplicationDirectory, toolName)
            ?? throw new WiimmsNotInstalledException(toolName);
    }

    private static string? FindConfiguredPath(string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return null;
        }

        return File.Exists(configuredPath) ? Path.GetFullPath(configuredPath) : null;
    }

    private string? FindInPath(string toolName)
    {
        if (string.IsNullOrWhiteSpace(_options.PathEnvironment))
        {
            return null;
        }

        foreach (var directory in _options.PathEnvironment.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var toolPath = FindInDirectory(directory, toolName);
            if (toolPath is not null)
            {
                return toolPath;
            }
        }

        return null;
    }

    private static string? FindInDirectory(string? directory, string toolName)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return null;
        }

        foreach (var candidateName in GetCandidateNames(toolName))
        {
            try
            {
                var candidatePath = Path.Combine(directory, candidateName);
                if (File.Exists(candidatePath))
                {
                    return Path.GetFullPath(candidatePath);
                }
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        return null;
    }

    private static IReadOnlyList<string> GetCandidateNames(string toolName)
    {
        if (OperatingSystem.IsWindows())
        {
            return [toolName, $"{toolName}.exe"];
        }

        return [toolName];
    }
}
