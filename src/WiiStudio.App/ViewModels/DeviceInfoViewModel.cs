using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class DeviceInfoViewModel
{
    public DeviceInfoViewModel(DeviceInfo device)
    {
        Device = device;
    }

    public DeviceInfo Device { get; }

    public string DeviceId => Device.DeviceId;

    public string Name => Device.Name;

    public string DevicePath => Device.DevicePath;

    public string MountPoint => Device.MountPoint;

    public string FileSystem => Device.FileSystem.ToString();

    public string Status => Device.Status.ToString();

    public string Compatibility => Device.IsWiiCompatible ? "Compatível" : "Incompatível";

    public string Capacity => FormatSize(Device.Capacity);

    public string FreeSpace => FormatSize(Device.FreeSpace);

    public string UsedSpace => FormatSize(Device.UsedSpace);

    public string Interface => Device.Interface;

    public string Model => Device.Model;

    private static string FormatSize(long sizeInBytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var size = (double)sizeInBytes;
        var unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }
}
