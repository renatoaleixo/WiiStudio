using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IOperatingSystemInfo
{
    OSPlatformKind Current { get; }
}
