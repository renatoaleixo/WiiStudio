using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record GameMetadataDto(
    string GameId,
    string Title,
    Region Region,
    string? Subtitle = null,
    string? Developer = null,
    string? Publisher = null,
    int? Year = null,
    IReadOnlyList<string>? Languages = null,
    IReadOnlyList<string>? Genres = null,
    int? Players = null,
    string? Rating = null,
    string? Description = null,
    string? OnlineFeatures = null,
    string? Compatibility = null,
    string? GameTdbVersion = null,
    DateTimeOffset? UpdatedAt = null);
