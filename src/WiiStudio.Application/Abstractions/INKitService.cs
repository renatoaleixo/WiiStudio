namespace WiiStudio.Application.Abstractions;

public interface INKitService
{
    Task<string> ConvertToIsoAsync(string sourcePath, string outputDirectory, CancellationToken cancellationToken);
}
