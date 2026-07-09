using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class OperatingSystemInfo : IOperatingSystemInfo
{
    public OSPlatformKind Current
    {
        get
        {
            if (OperatingSystem.IsLinux())
            {
                return OSPlatformKind.Linux;
            }

            if (OperatingSystem.IsWindows())
            {
                return OSPlatformKind.Windows;
            }

            if (OperatingSystem.IsMacOS())
            {
                return OSPlatformKind.MacOS;
            }

            return OSPlatformKind.Unknown;
        }
    }
}
