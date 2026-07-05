using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Domain.Entities;

public sealed class Cover
{
    public Cover(GameId gameId, string imagePath, string source, DateTimeOffset downloadedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        GameId = gameId;
        ImagePath = imagePath.Trim();
        Source = source.Trim();
        DownloadedAt = downloadedAt;
    }

    public GameId GameId { get; }

    public string ImagePath { get; }

    public string Source { get; }

    public DateTimeOffset DownloadedAt { get; }
}
