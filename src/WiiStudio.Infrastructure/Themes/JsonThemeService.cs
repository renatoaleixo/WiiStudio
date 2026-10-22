using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using AppSettings = WiiStudio.Domain.Entities.Settings;

namespace WiiStudio.Infrastructure.Themes;

public sealed class JsonThemeService : IThemeService
{
    private const string SystemThemeId = "System";
    private const string LightThemeId = "Light";
    private const string DarkThemeId = "Dark";

    private static readonly ThemeInfo[] BuiltInThemes =
    [
        new(SystemThemeId, "Sistema", ThemeKind.System, IsBuiltIn: true, IsActive: false),
        new(LightThemeId, "Claro", ThemeKind.Light, IsBuiltIn: true, IsActive: false),
        new(DarkThemeId, "Escuro", ThemeKind.Dark, IsBuiltIn: true, IsActive: false)
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly ISettingsService _settingsService;
    private readonly ThemeStoreOptions _options;

    public JsonThemeService(ISettingsService settingsService, ThemeStoreOptions options)
    {
        _settingsService = settingsService;
        _options = options;
    }

    public async Task<IReadOnlyList<ThemeInfo>> GetThemesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var settings = await _settingsService.GetAsync(cancellationToken);
        var activeThemeId = NormalizeThemeId(settings.Theme);
        var customThemes = await LoadCustomThemesAsync(cancellationToken);

        return BuiltInThemes
            .Concat(customThemes)
            .Select(theme => theme with { IsActive = theme.Id.Equals(activeThemeId, StringComparison.OrdinalIgnoreCase) })
            .ToArray();
    }

    public async Task<ThemeSnapshot> ApplyThemeAsync(string themeId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        cancellationToken.ThrowIfCancellationRequested();

        var themes = await GetThemesAsync(cancellationToken);
        var selected = themes.FirstOrDefault(theme => theme.Id.Equals(themeId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Theme '{themeId}' was not found.");
        var settings = await _settingsService.GetAsync(cancellationToken);

        await _settingsService.SaveAsync(CopySettingsWithTheme(settings, selected.Id), cancellationToken);

        var refreshedThemes = (await GetThemesAsync(cancellationToken)).ToArray();
        var active = refreshedThemes.First(theme => theme.IsActive);

        return new ThemeSnapshot(refreshedThemes, active, ThemeEngineState.Ready, DateTimeOffset.UtcNow);
    }

    public async Task<ThemeInfo> ImportThemeAsync(string filePath, CancellationToken cancellationToken)
    {
        ValidateFilePath(filePath, nameof(filePath));
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Theme import file was not found.", filePath);
        }

        await using var stream = File.OpenRead(filePath);
        var theme = await JsonSerializer.DeserializeAsync<ThemeInfo>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Theme import file is empty.");
        ValidateCustomTheme(theme);

        Directory.CreateDirectory(_options.ThemeDirectoryPath);
        var imported = theme with
        {
            Kind = ThemeKind.Custom,
            IsBuiltIn = false,
            IsActive = false,
            SourcePath = GetThemePath(theme.Id)
        };

        await SaveThemeAsync(imported, cancellationToken);
        return imported;
    }

    public async Task ExportThemeAsync(string themeId, string filePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        ValidateFilePath(filePath, nameof(filePath));
        cancellationToken.ThrowIfCancellationRequested();

        var theme = (await GetThemesAsync(cancellationToken))
            .FirstOrDefault(candidate => candidate.Id.Equals(themeId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Theme '{themeId}' was not found.");
        var exportTheme = theme with { IsActive = false, SourcePath = null };
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, exportTheme, JsonOptions, cancellationToken);
    }

    private async Task<IReadOnlyList<ThemeInfo>> LoadCustomThemesAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_options.ThemeDirectoryPath))
        {
            return [];
        }

        var themes = new List<ThemeInfo>();
        foreach (var filePath in Directory.EnumerateFiles(_options.ThemeDirectoryPath, "*.theme.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = File.OpenRead(filePath);
            var theme = await JsonSerializer.DeserializeAsync<ThemeInfo>(stream, JsonOptions, cancellationToken);
            if (theme is not null && IsValidCustomTheme(theme))
            {
                themes.Add(theme with { Kind = ThemeKind.Custom, IsBuiltIn = false, SourcePath = filePath });
            }
        }

        return themes;
    }

    private async Task SaveThemeAsync(ThemeInfo theme, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(GetThemePath(theme.Id));
        await JsonSerializer.SerializeAsync(stream, theme, JsonOptions, cancellationToken);
    }

    private string GetThemePath(string themeId)
    {
        return Path.Combine(_options.ThemeDirectoryPath, $"{NormalizeThemeId(themeId)}.theme.json");
    }

    private static string NormalizeThemeId(string themeId)
    {
        return themeId.Trim();
    }

    private static void ValidateCustomTheme(ThemeInfo theme)
    {
        if (!IsValidCustomTheme(theme))
        {
            throw new InvalidOperationException("Theme file is invalid.");
        }
    }

    private static bool IsValidCustomTheme(ThemeInfo theme)
    {
        return !string.IsNullOrWhiteSpace(theme.Id)
            && !string.IsNullOrWhiteSpace(theme.Name)
            && !BuiltInThemes.Any(builtIn => builtIn.Id.Equals(theme.Id, StringComparison.OrdinalIgnoreCase));
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

    private static AppSettings CopySettingsWithTheme(AppSettings settings, string themeId)
    {
        return new AppSettings
        {
            Language = settings.Language,
            Theme = themeId,
            AutoScanLibrary = settings.AutoScanLibrary,
            LibraryPath = settings.LibraryPath,
            ExportPath = settings.ExportPath,
            MaxParallelTasks = settings.MaxParallelTasks,
            EnableDiagnostics = settings.EnableDiagnostics,
            CheckForUpdatesOnStartup = settings.CheckForUpdatesOnStartup,
            WitExecutablePath = settings.WitExecutablePath,
            WwtExecutablePath = settings.WwtExecutablePath,
            NKitExecutablePath = settings.NKitExecutablePath,
            DolphinExecutablePath = settings.DolphinExecutablePath,
            WitVersion = settings.WitVersion,
            WwtVersion = settings.WwtVersion,
            NKitVersion = settings.NKitVersion,
            DolphinVersion = settings.DolphinVersion,
            EmulatorShowPreviewBeforeLaunch = settings.EmulatorShowPreviewBeforeLaunch,
            EmulatorSaveLaunchHistory = settings.EmulatorSaveLaunchHistory,
            EmulatorCloseAfterLaunch = settings.EmulatorCloseAfterLaunch,
            EmulatorLastId = settings.EmulatorLastId,
            EmulatorLastGameId = settings.EmulatorLastGameId,
            EmulatorDefaultProfileId = settings.EmulatorDefaultProfileId,
            EmulatorLastUsedProfileId = settings.EmulatorLastUsedProfileId,
            ExternalToolsLastCheckAt = settings.ExternalToolsLastCheckAt,
            CoverCachePath = settings.CoverCachePath,
            MetadataCachePath = settings.MetadataCachePath,
            FirstRunCompleted = settings.FirstRunCompleted,
            FirstRunCompletedAt = settings.FirstRunCompletedAt
        };
    }
}
