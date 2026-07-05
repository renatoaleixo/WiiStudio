using WiiStudio.Domain.Enums;

namespace WiiStudio.Domain.Entities;

public sealed class Game
{
    public Game(string id, string title, GameFormat format, long sizeInBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeInBytes);

        Id = id.Trim();
        Title = title.Trim();
        Format = format;
        SizeInBytes = sizeInBytes;
    }

    public string Id { get; }

    public string Title { get; }

    public GameFormat Format { get; }

    public long SizeInBytes { get; }
}
