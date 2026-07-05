using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Export;

public sealed class PhysicalExportFileSystem : IExportFileSystem
{
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public void EnsureDirectoryExists(string path)
    {
        Directory.CreateDirectory(path);
    }
}
