namespace WiiStudio.Infrastructure.Covers;

public interface IImageValidator
{
    bool IsValidImage(ReadOnlySpan<byte> content, string? contentType);
}
