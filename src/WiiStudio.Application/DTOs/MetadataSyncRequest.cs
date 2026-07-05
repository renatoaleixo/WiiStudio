namespace WiiStudio.Application.DTOs;

public sealed record MetadataSyncRequest(
    IReadOnlyList<string> GameIds,
    bool ForceRefresh = false);
