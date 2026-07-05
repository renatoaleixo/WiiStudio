using WiiStudio.Domain.Enums;

namespace WiiStudio.Domain.Entities;

public sealed class GameFile
{
    public GameFile(string path, FileFormat format, long sizeInBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeInBytes);

        Path = path.Trim();
        Format = format;
        SizeInBytes = sizeInBytes;
    }

    public string Path { get; }

    public FileFormat Format { get; }

    public long SizeInBytes { get; }
}
