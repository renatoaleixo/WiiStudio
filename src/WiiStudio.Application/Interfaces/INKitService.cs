namespace WiiStudio.Application.Interfaces;

public interface INKitService
{
    Task<string> ConvertToIsoAsync(string sourcePath, string outputDirectory, CancellationToken cancellationToken);
}
