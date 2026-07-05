namespace WiiStudio.Application.DTOs;

public enum MetadataSyncStatus
{
    Cached = 1,
    Updated = 2,
    Missing = 3,
    ProviderUnavailable = 4,
    Failed = 5
}
