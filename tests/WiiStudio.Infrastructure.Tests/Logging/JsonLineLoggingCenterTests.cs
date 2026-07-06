using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Infrastructure.Logging;

namespace WiiStudio.Infrastructure.Tests.Logging;

public sealed class JsonLineLoggingCenterTests
{
    [Fact]
    public async Task RecordAsync_ShouldPersistLogEntry()
    {
        using var fixture = TemporaryLoggingFixture.Create();
        var center = fixture.CreateCenter();
        var entry = CreateEntry(LogLevel.Information, "Application", "Startup", "Ready.");

        await center.RecordAsync(entry, CancellationToken.None);

        var logs = await center.QueryAsync(new LogQuery(), CancellationToken.None);
        logs.Should().ContainSingle().Which.Message.Should().Be("Ready.");
    }

    [Fact]
    public async Task QueryAsync_ShouldFilterByLevelSourceCategoryAndSearchText()
    {
        using var fixture = TemporaryLoggingFixture.Create();
        var center = fixture.CreateCenter();
        await center.RecordAsync(CreateEntry(LogLevel.Information, "Application", "Startup", "Ready."), CancellationToken.None);
        await center.RecordAsync(CreateEntry(LogLevel.Error, "Wiimms", "Verify", "Verification failed."), CancellationToken.None);

        var logs = await center.QueryAsync(
            new LogQuery(LogLevel.Warning, Source: "Wiimms", Category: "Verify", SearchText: "failed"),
            CancellationToken.None);

        logs.Should().ContainSingle().Which.Source.Should().Be("Wiimms");
    }

    [Fact]
    public async Task QueryAsync_ShouldFilterByOperationAndPeriod()
    {
        using var fixture = TemporaryLoggingFixture.Create();
        var center = fixture.CreateCenter();
        var operationId = Guid.NewGuid().ToString("N");
        await center.RecordAsync(CreateEntry(LogLevel.Information, "Application", "Import", "Old.", DateTimeOffset.UtcNow.AddHours(-2), operationId), CancellationToken.None);
        await center.RecordAsync(CreateEntry(LogLevel.Information, "Application", "Import", "Current.", DateTimeOffset.UtcNow, operationId), CancellationToken.None);

        var logs = await center.QueryAsync(
            new LogQuery(OperationId: operationId, From: DateTimeOffset.UtcNow.AddMinutes(-10)),
            CancellationToken.None);

        logs.Should().ContainSingle().Which.Message.Should().Be("Current.");
    }

    [Fact]
    public async Task QueryAsync_ShouldRespectLimitAndReturnNewestFirst()
    {
        using var fixture = TemporaryLoggingFixture.Create();
        var center = fixture.CreateCenter();
        await center.RecordAsync(CreateEntry(LogLevel.Information, "Application", "Test", "Older.", DateTimeOffset.UtcNow.AddMinutes(-1)), CancellationToken.None);
        await center.RecordAsync(CreateEntry(LogLevel.Information, "Application", "Test", "Newer.", DateTimeOffset.UtcNow), CancellationToken.None);

        var logs = await center.QueryAsync(new LogQuery(Limit: 1), CancellationToken.None);

        logs.Should().ContainSingle().Which.Message.Should().Be("Newer.");
    }

    [Fact]
    public async Task ExportAsync_ShouldWriteFilteredLogsToJson()
    {
        using var fixture = TemporaryLoggingFixture.Create();
        var center = fixture.CreateCenter();
        var exportPath = Path.Combine(fixture.RootDirectory, "export.json");
        await center.RecordAsync(CreateEntry(LogLevel.Error, "Application", "Backup", "Backup failed."), CancellationToken.None);

        await center.ExportAsync(new LogExportOptions(exportPath, new LogQuery(LogLevel.Error)), CancellationToken.None);

        File.Exists(exportPath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(exportPath);
        content.Should().Contain("Backup failed.");
    }

    [Fact]
    public async Task ClearAsync_ShouldRemoveEntriesByRetentionPolicy()
    {
        using var fixture = TemporaryLoggingFixture.Create();
        var center = fixture.CreateCenter();
        await center.RecordAsync(CreateEntry(LogLevel.Information, "Application", "Test", "Old.", DateTimeOffset.UtcNow.AddDays(-2)), CancellationToken.None);
        await center.RecordAsync(CreateEntry(LogLevel.Error, "Application", "Test", "Current.", DateTimeOffset.UtcNow), CancellationToken.None);

        await center.ClearAsync(new LogCleanupOptions(OlderThan: DateTimeOffset.UtcNow.AddDays(-1)), CancellationToken.None);

        var logs = await center.QueryAsync(new LogQuery(), CancellationToken.None);
        logs.Should().ContainSingle().Which.Message.Should().Be("Current.");
    }

    [Fact]
    public async Task ClearAsync_ShouldDeleteLogFileWhenNoPolicyIsProvided()
    {
        using var fixture = TemporaryLoggingFixture.Create();
        var center = fixture.CreateCenter();
        await center.RecordAsync(CreateEntry(LogLevel.Information, "Application", "Test", "Ready."), CancellationToken.None);

        await center.ClearAsync(new LogCleanupOptions(), CancellationToken.None);

        File.Exists(fixture.LogPath).Should().BeFalse();
    }

    [Fact]
    public async Task RecordAsync_ShouldMaskSensitiveValues()
    {
        using var fixture = TemporaryLoggingFixture.Create();
        var center = fixture.CreateCenter();
        var entry = CreateEntry(LogLevel.Information, "Application", "Auth", "token=abc123")
            with { Properties = new Dictionary<string, string> { ["password"] = "secret-value" } };

        await center.RecordAsync(entry, CancellationToken.None);

        var content = await File.ReadAllTextAsync(fixture.LogPath);
        content.Should().Contain("token=***");
        content.Should().Contain("\"password\":\"***\"");
        content.Should().NotContain("abc123");
        content.Should().NotContain("secret-value");
    }

    [Fact]
    public async Task RecordAsync_ShouldRejectInvalidEntry()
    {
        using var fixture = TemporaryLoggingFixture.Create();
        var center = fixture.CreateCenter();

        var act = () => center.RecordAsync(CreateEntry(LogLevel.Information, "", "Test", "Ready."), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private static LogEntry CreateEntry(
        LogLevel level,
        string source,
        string category,
        string message,
        DateTimeOffset? timestamp = null,
        string? operationId = null)
    {
        return new LogEntry(
            Guid.NewGuid(),
            timestamp ?? DateTimeOffset.UtcNow,
            level,
            source,
            category,
            message,
            operationId);
    }

    private sealed class TemporaryLoggingFixture : IDisposable
    {
        private TemporaryLoggingFixture(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            LogPath = Path.Combine(rootDirectory, "logs", "wiistudio.jsonl");
        }

        public string RootDirectory { get; }

        public string LogPath { get; }

        public static TemporaryLoggingFixture Create()
        {
            return new TemporaryLoggingFixture(Path.Combine(Path.GetTempPath(), $"wiistudio-logs-{Guid.NewGuid():N}"));
        }

        public JsonLineLoggingCenter CreateCenter()
        {
            return new JsonLineLoggingCenter(new LoggingCenterOptions
            {
                FilePath = LogPath,
                DefaultQueryLimit = 500
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
