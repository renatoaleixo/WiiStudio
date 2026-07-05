namespace WiiStudio.Application.DTOs;

public sealed record SearchPreset(
    Guid Id,
    string Name,
    SearchRequest Request,
    DateTimeOffset CreatedAt);
