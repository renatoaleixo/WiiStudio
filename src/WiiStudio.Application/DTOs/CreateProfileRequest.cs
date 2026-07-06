using WiiStudio.Domain.Entities;

namespace WiiStudio.Application.DTOs;

public sealed record CreateProfileRequest(
    string Name,
    Settings? Settings = null);
