using System.Text.Json;
using WiiStudio.Application.Interfaces;
using AppSettings = WiiStudio.Domain.Entities.Settings;

namespace WiiStudio.Infrastructure.Settings;

public sealed class JsonSettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly SettingsStoreOptions _options;

    public JsonSettingsService(SettingsStoreOptions options)
    {
        _options = options;
    }

    public Task<AppSettings> GetAsync(CancellationToken cancellationToken)
    {
        return LoadAsync(cancellationToken);
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_options.FilePath))
        {
            return new AppSettings();
        }

        await using var stream = File.OpenRead(_options.FilePath);
        var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, JsonOptions, cancellationToken);

        return settings ?? new AppSettings();
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();

        Validate(settings);
        await SaveToPathAsync(_options.FilePath, settings, cancellationToken);
    }

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        await SaveAsync(new AppSettings(), cancellationToken);
    }

    public async Task ExportAsync(string filePath, CancellationToken cancellationToken)
    {
        ValidateFilePath(filePath, nameof(filePath));
        cancellationToken.ThrowIfCancellationRequested();

        var settings = await LoadAsync(cancellationToken);
        await SaveToPathAsync(filePath, settings, cancellationToken);
    }

    public async Task ImportAsync(string filePath, CancellationToken cancellationToken)
    {
        ValidateFilePath(filePath, nameof(filePath));
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Settings import file was not found.", filePath);
        }

        await using var stream = File.OpenRead(filePath);
        var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Settings import file is empty.");
        Validate(settings);

        await BackupCurrentSettingsAsync(cancellationToken);
        await SaveAsync(settings, cancellationToken);
    }

    private async Task BackupCurrentSettingsAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_options.FilePath))
        {
            return;
        }

        var backupPath = $"{_options.FilePath}.bak";
        await using var source = File.OpenRead(_options.FilePath);
        await using var destination = File.Create(backupPath);
        await source.CopyToAsync(destination, cancellationToken);
    }

    private static async Task SaveToPathAsync(string filePath, AppSettings settings, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
    }

    private static void Validate(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Language))
        {
            throw new ArgumentException("Language must be provided.", nameof(settings));
        }

        if (string.IsNullOrWhiteSpace(settings.Theme))
        {
            throw new ArgumentException("Theme must be provided.", nameof(settings));
        }

        if (settings.MaxParallelTasks <= 0)
        {
            throw new ArgumentException("MaxParallelTasks must be greater than zero.", nameof(settings));
        }

        ValidateOptionalPath(settings.LibraryPath, nameof(settings.LibraryPath));
        ValidateOptionalPath(settings.ExportPath, nameof(settings.ExportPath));
        ValidateOptionalPath(settings.WitExecutablePath, nameof(settings.WitExecutablePath));
        ValidateOptionalPath(settings.WwtExecutablePath, nameof(settings.WwtExecutablePath));
        ValidateOptionalPath(settings.NKitExecutablePath, nameof(settings.NKitExecutablePath));
        ValidateOptionalPath(settings.CoverCachePath, nameof(settings.CoverCachePath));
        ValidateOptionalPath(settings.MetadataCachePath, nameof(settings.MetadataCachePath));
    }

    private static void ValidateOptionalPath(string? path, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        ValidateFilePath(path, parameterName);
    }

    private static void ValidateFilePath(string filePath, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Path must be provided.", parameterName);
        }

        if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new ArgumentException("Path contains invalid characters.", parameterName);
        }
    }
}
