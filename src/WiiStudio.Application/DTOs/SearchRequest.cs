using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record SearchRequest(
    Guid? UsbDriveId = null,
    string? SearchText = null,
    IReadOnlySet<Region>? Regions = null,
    IReadOnlySet<FileFormat>? Formats = null,
    bool FavoritesOnly = false,
    long? MinimumSizeInBytes = null,
    long? MaximumSizeInBytes = null,
    GameSortField SortField = GameSortField.Title,
    bool SortDescending = false);
