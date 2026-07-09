namespace WiiStudio.Infrastructure.Privilege;

public interface IWindowsElevationDetector
{
    bool IsAdministrator();
}
