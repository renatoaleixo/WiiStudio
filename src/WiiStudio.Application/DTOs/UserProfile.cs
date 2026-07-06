using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.DTOs;

public sealed record UserProfile(
    Guid Id,
    string Name,
    Settings Settings,
    bool IsDefault,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
