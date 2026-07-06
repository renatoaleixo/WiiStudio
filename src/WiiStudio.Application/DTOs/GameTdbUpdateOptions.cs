namespace WiiStudio.Application.DTOs;

public sealed record GameTdbUpdateOptions(
    bool Force = false,
    bool PreferIncremental = true);
