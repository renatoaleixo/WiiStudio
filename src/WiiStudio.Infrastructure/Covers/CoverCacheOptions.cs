namespace WiiStudio.Infrastructure.Covers;

public sealed class CoverCacheOptions
{
    public string RootDirectory { get; init; } = Path.Combine(AppContext.BaseDirectory, "assets", "cache");
}
