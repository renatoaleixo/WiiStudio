namespace WiiStudio.Application.DTOs;

public sealed record BackupPreview(
    IReadOnlyList<BackupItem> Items,
    long TotalSizeInBytes,
    DateTimeOffset AnalyzedAt);
