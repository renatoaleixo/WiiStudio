using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class CommandPaletteServiceTests
{
    [Fact]
    public async Task SearchAsync_ShouldFindCommandsByTitleAndAlias()
    {
        var service = new CommandPaletteService(new FakeCommandExecutor(), new FakeCommandHistoryStore());

        var importResults = await service.SearchAsync("import", CancellationToken.None);
        var aliasResults = await service.SearchAsync("usb", CancellationToken.None);

        importResults.Should().Contain(command => command.Id == "import.open");
        aliasResults.Should().Contain(command => command.Id == "devices.refresh");
    }

    [Fact]
    public async Task SearchAsync_ShouldSupportFuzzySubsequence()
    {
        var service = new CommandPaletteService(new FakeCommandExecutor(), new FakeCommandHistoryStore());

        var results = await service.SearchAsync("dsh", CancellationToken.None);

        results.Should().Contain(command => command.Id == "navigation.dashboard");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRunCommandAndUpdateRecentHistory()
    {
        var executor = new FakeCommandExecutor();
        var history = new FakeCommandHistoryStore();
        var service = new CommandPaletteService(executor, history);

        await service.ExecuteAsync("library.refresh", CancellationToken.None);

        executor.ExecutedCommands.Should().ContainSingle().Which.Should().Be("library.refresh");
        var recent = await service.GetRecentCommandsAsync(CancellationToken.None);
        recent.Should().ContainSingle().Which.Id.Should().Be("library.refresh");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectUnknownCommand()
    {
        var service = new CommandPaletteService(new FakeCommandExecutor(), new FakeCommandHistoryStore());

        var act = () => service.ExecuteAsync("missing", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private sealed class FakeCommandExecutor : ICommandExecutor
    {
        public List<string> ExecutedCommands { get; } = [];

        public Task ExecuteAsync(string commandId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutedCommands.Add(commandId);

            return Task.CompletedTask;
        }
    }

    private sealed class FakeCommandHistoryStore : ICommandHistoryStore
    {
        private IReadOnlyList<string> _commandIds = [];

        public Task<IReadOnlyList<string>> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_commandIds);
        }

        public Task SaveAsync(IReadOnlyList<string> commandIds, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _commandIds = commandIds;

            return Task.CompletedTask;
        }
    }
}
