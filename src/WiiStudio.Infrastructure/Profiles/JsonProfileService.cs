using System.Text.Json;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using AppSettings = WiiStudio.Domain.Entities.Settings;

namespace WiiStudio.Infrastructure.Profiles;

public sealed class JsonProfileService : IProfileService
{
    private static readonly Guid DefaultProfileId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string ProfileFileName = "profile.json";
    private const string StateFileName = "profiles-state.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly ProfileStoreOptions _options;

    public JsonProfileService(ProfileStoreOptions options)
    {
        _options = options;
    }

    public async Task<IReadOnlyList<UserProfile>> GetProfilesAsync(CancellationToken cancellationToken)
    {
        var state = await EnsureStateAsync(cancellationToken);
        var profiles = await LoadProfilesAsync(cancellationToken);

        return profiles
            .Select(profile => ToUserProfile(profile, profile.Id == state.ActiveProfileId))
            .OrderByDescending(profile => profile.IsDefault)
            .ThenBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<UserProfile> CreateAsync(CreateProfileRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateProfileName(request.Name);
        var profiles = await LoadProfilesAsync(cancellationToken);
        EnsureUniqueName(profiles, request.Name);

        var now = DateTimeOffset.UtcNow;
        var profile = new StoredProfile(
            Guid.NewGuid(),
            request.Name.Trim(),
            request.Settings ?? new AppSettings(),
            IsDefault: false,
            now,
            now);
        await SaveProfileAsync(profile, cancellationToken);
        var state = await EnsureStateAsync(cancellationToken);

        return ToUserProfile(profile, profile.Id == state.ActiveProfileId);
    }

    public async Task<UserProfile> DuplicateAsync(Guid profileId, string name, CancellationToken cancellationToken)
    {
        ValidateProfileName(name);
        var profiles = await LoadProfilesAsync(cancellationToken);
        var source = FindProfile(profiles, profileId);
        EnsureUniqueName(profiles, name);

        var now = DateTimeOffset.UtcNow;
        var profile = source with
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            IsDefault = false,
            CreatedAt = now,
            UpdatedAt = now
        };
        await SaveProfileAsync(profile, cancellationToken);

        return ToUserProfile(profile, isActive: false);
    }

    public async Task<UserProfile> RenameAsync(Guid profileId, string name, CancellationToken cancellationToken)
    {
        ValidateProfileName(name);
        var profiles = await LoadProfilesAsync(cancellationToken);
        var profile = FindProfile(profiles, profileId);
        EnsureUniqueName(profiles.Where(candidate => candidate.Id != profileId), name);

        var renamed = profile with { Name = name.Trim(), UpdatedAt = DateTimeOffset.UtcNow };
        await SaveProfileAsync(renamed, cancellationToken);
        var state = await EnsureStateAsync(cancellationToken);

        return ToUserProfile(renamed, renamed.Id == state.ActiveProfileId);
    }

    public async Task SetActiveAsync(Guid profileId, CancellationToken cancellationToken)
    {
        var profiles = await LoadProfilesAsync(cancellationToken);
        FindProfile(profiles, profileId);
        await SaveStateAsync(new ProfileState(profileId), cancellationToken);
    }

    public async Task DeleteAsync(Guid profileId, CancellationToken cancellationToken)
    {
        var profiles = await LoadProfilesAsync(cancellationToken);
        var profile = FindProfile(profiles, profileId);
        if (profile.IsDefault)
        {
            throw new InvalidOperationException("Default profile cannot be deleted.");
        }

        var directory = GetProfileDirectory(profile.Id);
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        var state = await EnsureStateAsync(cancellationToken);
        if (state.ActiveProfileId == profileId)
        {
            await SaveStateAsync(new ProfileState(DefaultProfileId), cancellationToken);
        }
    }

    public async Task ExportAsync(Guid profileId, string filePath, CancellationToken cancellationToken)
    {
        ValidatePath(filePath, nameof(filePath));
        var profiles = await LoadProfilesAsync(cancellationToken);
        var profile = FindProfile(profiles, profileId);
        await SaveToPathAsync(filePath, profile, cancellationToken);
    }

    public async Task<UserProfile> ImportAsync(string filePath, CancellationToken cancellationToken)
    {
        ValidatePath(filePath, nameof(filePath));
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Profile import file was not found.", filePath);
        }

        await using var stream = File.OpenRead(filePath);
        var imported = await JsonSerializer.DeserializeAsync<StoredProfile>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Profile import file is empty.");
        ValidateProfileName(imported.Name);

        var profiles = await LoadProfilesAsync(cancellationToken);
        var importedName = GetUniqueImportedName(profiles, imported.Name);
        var now = DateTimeOffset.UtcNow;
        var profile = imported with
        {
            Id = Guid.NewGuid(),
            Name = importedName,
            IsDefault = false,
            CreatedAt = now,
            UpdatedAt = now
        };
        await SaveProfileAsync(profile, cancellationToken);

        return ToUserProfile(profile, isActive: false);
    }

    private async Task<IReadOnlyList<StoredProfile>> LoadProfilesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await EnsureDefaultProfileAsync(cancellationToken);

        return Directory
            .EnumerateFiles(_options.RootDirectory, ProfileFileName, SearchOption.AllDirectories)
            .Select(path => LoadProfile(path, cancellationToken))
            .ToArray();
    }

    private StoredProfile LoadProfile(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var stream = File.OpenRead(filePath);
        var profile = JsonSerializer.Deserialize<StoredProfile>(stream, JsonOptions);

        return profile ?? throw new InvalidOperationException($"Profile file is empty: {filePath}");
    }

    private async Task<ProfileState> EnsureStateAsync(CancellationToken cancellationToken)
    {
        await EnsureDefaultProfileAsync(cancellationToken);
        var statePath = GetStatePath();
        if (!File.Exists(statePath))
        {
            var defaultState = new ProfileState(DefaultProfileId);
            await SaveStateAsync(defaultState, cancellationToken);
            return defaultState;
        }

        await using var stream = File.OpenRead(statePath);
        var state = await JsonSerializer.DeserializeAsync<ProfileState>(stream, JsonOptions, cancellationToken);
        return state ?? new ProfileState(DefaultProfileId);
    }

    private async Task EnsureDefaultProfileAsync(CancellationToken cancellationToken)
    {
        var defaultPath = GetProfilePath(DefaultProfileId);
        if (File.Exists(defaultPath))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var profile = new StoredProfile(DefaultProfileId, "Default", new AppSettings(), IsDefault: true, now, now);
        await SaveProfileAsync(profile, cancellationToken);
    }

    private async Task SaveProfileAsync(StoredProfile profile, CancellationToken cancellationToken)
    {
        await SaveToPathAsync(GetProfilePath(profile.Id), profile, cancellationToken);
    }

    private async Task SaveStateAsync(ProfileState state, CancellationToken cancellationToken)
    {
        await SaveToPathAsync(GetStatePath(), state, cancellationToken);
    }

    private static async Task SaveToPathAsync<T>(string filePath, T value, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, value, JsonOptions, cancellationToken);
    }

    private string GetProfileDirectory(Guid profileId)
    {
        return Path.Combine(_options.RootDirectory, profileId.ToString("N"));
    }

    private string GetProfilePath(Guid profileId)
    {
        return Path.Combine(GetProfileDirectory(profileId), ProfileFileName);
    }

    private string GetStatePath()
    {
        return Path.Combine(_options.RootDirectory, StateFileName);
    }

    private static UserProfile ToUserProfile(StoredProfile profile, bool isActive)
    {
        return new UserProfile(
            profile.Id,
            profile.Name,
            profile.Settings,
            profile.IsDefault,
            isActive,
            profile.CreatedAt,
            profile.UpdatedAt);
    }

    private static StoredProfile FindProfile(IEnumerable<StoredProfile> profiles, Guid profileId)
    {
        return profiles.FirstOrDefault(profile => profile.Id == profileId)
            ?? throw new KeyNotFoundException($"Profile '{profileId}' was not found.");
    }

    private static void EnsureUniqueName(IEnumerable<StoredProfile> profiles, string name)
    {
        if (profiles.Any(profile => string.Equals(profile.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("A profile with the same name already exists.");
        }
    }

    private static string GetUniqueImportedName(IEnumerable<StoredProfile> profiles, string name)
    {
        var trimmed = name.Trim();
        if (!profiles.Any(profile => string.Equals(profile.Name, trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            return trimmed;
        }

        var index = 1;
        while (true)
        {
            var candidate = $"{trimmed} (Imported {index})";
            if (!profiles.Any(profile => string.Equals(profile.Name, candidate, StringComparison.OrdinalIgnoreCase)))
            {
                return candidate;
            }

            index++;
        }
    }

    private static void ValidateProfileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Profile name must be provided.", nameof(name));
        }
    }

    private static void ValidatePath(string filePath, string parameterName)
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

    private sealed record StoredProfile(
        Guid Id,
        string Name,
        AppSettings Settings,
        bool IsDefault,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    private sealed record ProfileState(Guid ActiveProfileId);
}
