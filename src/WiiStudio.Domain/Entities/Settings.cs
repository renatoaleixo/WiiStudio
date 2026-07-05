namespace WiiStudio.Domain.Entities;

public sealed class Settings
{
    public string? WitExecutablePath { get; init; }

    public string? WwtExecutablePath { get; init; }

    public string? NKitExecutablePath { get; init; }

    public string? CoverCachePath { get; init; }
}
