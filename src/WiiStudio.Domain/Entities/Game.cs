using WiiStudio.Domain.Enums;
using WiiStudio.Domain.ValueObjects;

namespace WiiStudio.Domain.Entities;

public sealed class Game
{
    public Game(string id, string title, GameFormat format, long sizeInBytes)
        : this(new GameId(id), title, MapFormat(format), sizeInBytes)
    {
    }

    public Game(GameId gameId, string title, FileFormat format, long sizeInBytes)
        : this(gameId, title, Region.Unknown, DiscType.Wii, format, null, sizeInBytes, DateTimeOffset.UtcNow)
    {
    }

    public Game(
        GameId gameId,
        string title,
        Region region,
        DiscType discType,
        FileFormat format,
        string? filePath,
        long sizeInBytes,
        DateTimeOffset addedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeInBytes);

        EntityId = Guid.NewGuid();
        GameId = gameId;
        Title = title.Trim();
        Format = format;
        Region = region;
        DiscType = discType;
        FilePath = string.IsNullOrWhiteSpace(filePath) ? null : filePath.Trim();
        SizeInBytes = sizeInBytes;
        AddedAt = addedAt;
    }

    public Guid EntityId { get; }

    public string Id => GameId.Value;

    public GameId GameId { get; }

    public string Title { get; }

    public Region Region { get; }

    public DiscType DiscType { get; }

    public FileFormat Format { get; }

    public string? FilePath { get; }

    public long SizeInBytes { get; }

    public DateTimeOffset AddedAt { get; }

    private static FileFormat MapFormat(GameFormat format)
    {
        return format switch
        {
            GameFormat.Iso => FileFormat.Iso,
            GameFormat.Wbfs => FileFormat.Wbfs,
            GameFormat.Rvz => FileFormat.Rvz,
            GameFormat.NKit => FileFormat.NKit,
            _ => FileFormat.Unknown
        };
    }
}
