namespace WiiStudio.Application.DTOs;

public sealed record BackupItem(
    BackupItemKind Kind,
    string RelativePath,
    long SizeInBytes,
    bool Exists);
