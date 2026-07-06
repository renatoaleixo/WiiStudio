using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.AppUpdates;

namespace WiiStudio.Infrastructure.Tests.AppUpdates;

public sealed class JsonApplicationUpdateStateStoreTests
{
    [Fact]
    public async Task LoadAsync_ShouldReturnConfiguredCurrentVersionWhenFileDoesNotExist()
    {
        using var fixture = TemporaryStateStoreFixture.Create();
        var store = fixture.CreateStore(currentVersion: "1.0.0");

        var state = await store.LoadAsync(CancellationToken.None);

        state.CurrentVersion.Should().Be("1.0.0");
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistState()
    {
        using var fixture = TemporaryStateStoreFixture.Create();
        var store = fixture.CreateStore(currentVersion: "1.0.0");

        await store.SaveAsync(new ApplicationUpdateState("1.1.0", PreviousVersion: "1.0.0", RestartPending: true), CancellationToken.None);
        var state = await store.LoadAsync(CancellationToken.None);

        state.CurrentVersion.Should().Be("1.1.0");
        state.PreviousVersion.Should().Be("1.0.0");
        state.RestartPending.Should().BeTrue();
    }

    private sealed class TemporaryStateStoreFixture : IDisposable
    {
        private TemporaryStateStoreFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public string RootDirectory { get; }

        public static TemporaryStateStoreFixture Create()
        {
            return new TemporaryStateStoreFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-app-update-{Guid.NewGuid():N}"));
        }

        public JsonApplicationUpdateStateStore CreateStore(string currentVersion)
        {
            return new JsonApplicationUpdateStateStore(new ApplicationUpdateStateStoreOptions
            {
                FilePath = Path.Combine(RootDirectory, "state.json"),
                CurrentVersion = currentVersion
            });
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
