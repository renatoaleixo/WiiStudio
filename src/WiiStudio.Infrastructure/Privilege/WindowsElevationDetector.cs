using System.Security.Principal;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class WindowsElevationDetector : IWindowsElevationDetector
{
    public bool IsAdministrator()
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
