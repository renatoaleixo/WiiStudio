namespace WiiStudio.Application.Interfaces;

public interface IExportFileSystem
{
    bool FileExists(string path);

    void EnsureDirectoryExists(string path);
}
