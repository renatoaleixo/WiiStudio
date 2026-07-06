using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Profiles;
using AppSettings = WiiStudio.Domain.Entities.Settings;

namespace WiiStudio.Infrastructure.Tests.Profiles;

public sealed class JsonProfileServiceTests
{
    [Fact]
    public async Task GetProfilesAsync_ShouldCreateDefaultActiveProfile()
    {
        using var fixture = TemporaryProfileFixture.Create();
        var service = fixture.CreateService();

        var profiles = await service.GetProfilesAsync(CancellationToken.None);

        var profile = profiles.Should().ContainSingle().Subject;
        profile.Name.Should().Be("Default");
        profile.IsDefault.Should().BeTrue();
        profile.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistProfile()
    {
        using var fixture = TemporaryProfileFixture.Create();
        var service = fixture.CreateService();

        var profile = await service.CreateAsync(
            new CreateProfileRequest("Gaming", new AppSettings { Theme = "Dark" }),
            CancellationToken.None);
        var profiles = await service.GetProfilesAsync(CancellationToken.None);

        profile.Name.Should().Be("Gaming");
        profile.Settings.Theme.Should().Be("Dark");
        profiles.Should().Contain(candidate => candidate.Id == profile.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectDuplicateProfileName()
    {
        using var fixture = TemporaryProfileFixture.Create();
        var service = fixture.CreateService();
        await service.CreateAsync(new CreateProfileRequest("Gaming"), CancellationToken.None);

        var act = () => service.CreateAsync(new CreateProfileRequest("gaming"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetActiveAsync_ShouldActivateSelectedProfile()
    {
        using var fixture = TemporaryProfileFixture.Create();
        var service = fixture.CreateService();
        var profile = await service.CreateAsync(new CreateProfileRequest("Gaming"), CancellationToken.None);

        await service.SetActiveAsync(profile.Id, CancellationToken.None);

        var profiles = await service.GetProfilesAsync(CancellationToken.None);
        profiles.Single(candidate => candidate.Id == profile.Id).IsActive.Should().BeTrue();
        profiles.Single(candidate => candidate.IsDefault).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRejectDefaultProfile()
    {
        using var fixture = TemporaryProfileFixture.Create();
        var service = fixture.CreateService();
        var defaultProfile = (await service.GetProfilesAsync(CancellationToken.None)).Single();

        var act = () => service.DeleteAsync(defaultProfile.Id, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRestoreDefaultWhenActiveProfileIsDeleted()
    {
        using var fixture = TemporaryProfileFixture.Create();
        var service = fixture.CreateService();
        var profile = await service.CreateAsync(new CreateProfileRequest("Gaming"), CancellationToken.None);
        await service.SetActiveAsync(profile.Id, CancellationToken.None);

        await service.DeleteAsync(profile.Id, CancellationToken.None);

        var profiles = await service.GetProfilesAsync(CancellationToken.None);
        profiles.Should().NotContain(candidate => candidate.Id == profile.Id);
        profiles.Single(candidate => candidate.IsDefault).IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DuplicateAsync_ShouldCreateCopyWithNewName()
    {
        using var fixture = TemporaryProfileFixture.Create();
        var service = fixture.CreateService();
        var profile = await service.CreateAsync(
            new CreateProfileRequest("Gaming", new AppSettings { Language = "en-US" }),
            CancellationToken.None);

        var duplicate = await service.DuplicateAsync(profile.Id, "Gaming Copy", CancellationToken.None);

        duplicate.Id.Should().NotBe(profile.Id);
        duplicate.Name.Should().Be("Gaming Copy");
        duplicate.Settings.Language.Should().Be("en-US");
        duplicate.IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task RenameAsync_ShouldPersistNewName()
    {
        using var fixture = TemporaryProfileFixture.Create();
        var service = fixture.CreateService();
        var profile = await service.CreateAsync(new CreateProfileRequest("Gaming"), CancellationToken.None);

        var renamed = await service.RenameAsync(profile.Id, "Handheld", CancellationToken.None);

        renamed.Name.Should().Be("Handheld");
        var profiles = await service.GetProfilesAsync(CancellationToken.None);
        profiles.Should().Contain(candidate => candidate.Id == profile.Id && candidate.Name == "Handheld");
    }

    [Fact]
    public async Task ExportAsyncAndImportAsync_ShouldRoundTripProfileAsInactiveCopy()
    {
        using var fixture = TemporaryProfileFixture.Create();
        var service = fixture.CreateService();
        var exportPath = Path.Combine(fixture.RootDirectory, "exported-profile.json");
        var profile = await service.CreateAsync(
            new CreateProfileRequest("Gaming", new AppSettings { Theme = "Dark" }),
            CancellationToken.None);

        await service.ExportAsync(profile.Id, exportPath, CancellationToken.None);
        await service.DeleteAsync(profile.Id, CancellationToken.None);
        var imported = await service.ImportAsync(exportPath, CancellationToken.None);

        imported.Id.Should().NotBe(profile.Id);
        imported.Name.Should().Be("Gaming");
        imported.Settings.Theme.Should().Be("Dark");
        imported.IsActive.Should().BeFalse();
    }

    private sealed class TemporaryProfileFixture : IDisposable
    {
        private TemporaryProfileFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public string RootDirectory { get; }

        public static TemporaryProfileFixture Create()
        {
            return new TemporaryProfileFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-profiles-{Guid.NewGuid():N}"));
        }

        public JsonProfileService CreateService()
        {
            return new JsonProfileService(new ProfileStoreOptions { RootDirectory = RootDirectory });
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }
    }
}
