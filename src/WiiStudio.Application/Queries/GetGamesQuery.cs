using WiiStudio.Application.DTOs;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Queries;

public sealed record GetGamesQuery(
    Guid? UsbDriveId = null,
    string? SearchText = null,
    IReadOnlySet<Region>? Regions = null,
    IReadOnlySet<FileFormat>? Formats = null,
    long? MinimumSizeInBytes = null,
    long? MaximumSizeInBytes = null,
    GameSortField SortField = GameSortField.Title,
    bool SortDescending = false);
