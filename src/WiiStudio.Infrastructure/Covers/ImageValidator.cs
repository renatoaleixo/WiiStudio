namespace WiiStudio.Infrastructure.Covers;

public sealed class ImageValidator : IImageValidator
{
    public bool IsValidImage(ReadOnlySpan<byte> content, string? contentType)
    {
        if (content.IsEmpty ||
            string.IsNullOrWhiteSpace(contentType) ||
            !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return IsValidPng(content) || IsValidJpeg(content) || IsValidWebp(content);
    }

    private static bool IsValidPng(ReadOnlySpan<byte> content)
    {
        ReadOnlySpan<byte> signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        if (content.Length < 24 || !content[..8].SequenceEqual(signature))
        {
            return false;
        }

        var width = ReadBigEndianInt32(content[16..20]);
        var height = ReadBigEndianInt32(content[20..24]);

        return width > 0 && height > 0;
    }

    private static bool IsValidJpeg(ReadOnlySpan<byte> content)
    {
        if (content.Length < 4 || content[0] != 0xFF || content[1] != 0xD8)
        {
            return false;
        }

        var index = 2;
        while (index + 9 < content.Length)
        {
            if (content[index] != 0xFF)
            {
                index++;
                continue;
            }

            var marker = content[index + 1];
            if (marker is >= 0xC0 and <= 0xC3)
            {
                var height = ReadBigEndianUInt16(content[(index + 5)..(index + 7)]);
                var width = ReadBigEndianUInt16(content[(index + 7)..(index + 9)]);

                return width > 0 && height > 0;
            }

            var segmentLength = ReadBigEndianUInt16(content[(index + 2)..(index + 4)]);
            if (segmentLength <= 0)
            {
                return false;
            }

            index += 2 + segmentLength;
        }

        return false;
    }

    private static bool IsValidWebp(ReadOnlySpan<byte> content)
    {
        return content.Length > 16 &&
            content[..4].SequenceEqual("RIFF"u8) &&
            content[8..12].SequenceEqual("WEBP"u8);
    }

    private static int ReadBigEndianInt32(ReadOnlySpan<byte> bytes)
    {
        return bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];
    }

    private static int ReadBigEndianUInt16(ReadOnlySpan<byte> bytes)
    {
        return bytes[0] << 8 | bytes[1];
    }
}
