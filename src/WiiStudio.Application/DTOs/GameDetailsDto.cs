using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.DTOs;

public sealed record GameDetailsDto(
    string Id,
    string Title,
    Region Region,
    DiscType DiscType,
    FileFormat Format,
    string? FilePath,
    long SizeInBytes,
    DateTimeOffset AddedAt,
    bool IsFavorite,
    IReadOnlyList<GameDetailsAction> AvailableActions);
