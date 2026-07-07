using FluentAssertions;
using WiiStudio.Infrastructure.Commands;

namespace WiiStudio.Infrastructure.Tests.Commands;

public sealed class JsonCommandHistoryStoreTests
{
    [Fact]
    public async Task LoadAsync_ShouldReturnEmptyWhenFileDoesNotExist()
    {
        using var fixture = TemporaryCommandHistoryFixture.Create();
        var store = fixture.CreateStore();

        var commands = await store.LoadAsync(CancellationToken.None);

        commands.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsyncAndLoadAsync_ShouldRoundTripCommandIds()
    {
        using var fixture = TemporaryCommandHistoryFixture.Create();
        var store = fixture.CreateStore();

        await store.SaveAsync(["library.refresh", "import.open"], CancellationToken.None);
        var commands = await store.LoadAsync(CancellationToken.None);

        commands.Should().Equal("library.refresh", "import.open");
    }

    private sealed class TemporaryCommandHistoryFixture : IDisposable
    {
        private TemporaryCommandHistoryFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            FilePath = Path.Combine(rootDirectory, "recent-commands.json");
        }

        public string RootDirectory { get; }

        public string FilePath { get; }

        public static TemporaryCommandHistoryFixture Create()
        {
            return new TemporaryCommandHistoryFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-command-history-{Guid.NewGuid():N}"));
        }

        public JsonCommandHistoryStore CreateStore()
        {
            return new JsonCommandHistoryStore(new CommandHistoryStoreOptions { FilePath = FilePath });
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
