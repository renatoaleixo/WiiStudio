using WiiStudio.Application.ExternalTools;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.ExternalTools;

public sealed class NKitService : INKitService
{
    private static readonly TimeSpan RestoreTimeout = TimeSpan.FromHours(2);

    private readonly IExternalToolManager externalToolManager;
    private readonly IProcessRunner processRunner;

    public NKitService(IExternalToolManager externalToolManager, IProcessRunner processRunner)
    {
        this.externalToolManager = externalToolManager;
        this.processRunner = processRunner;
    }

    public async Task<string> ConvertToIsoAsync(string sourcePath, string outputDirectory, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        cancellationToken.ThrowIfCancellationRequested();

        var status = await externalToolManager.CheckAsync(ExternalToolDefinitionProvider.NKitToolId, cancellationToken);
        if (status.State is not (ExternalToolState.Installed or ExternalToolState.Configured) || string.IsNullOrWhiteSpace(status.ExecutablePath))
        {
            throw new InvalidOperationException("NKit is not configured or installed.");
        }

        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(sourcePath) + ".iso");
        var result = await processRunner.RunAsync(
            NKitProcessRequestFactory.Create(status.ExecutablePath, [sourcePath, outputDirectory], RestoreTimeout),
            cancellationToken);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"NKit restore failed with exit code {result.ExitCode}.");
        }

        return outputPath;
    }
}
