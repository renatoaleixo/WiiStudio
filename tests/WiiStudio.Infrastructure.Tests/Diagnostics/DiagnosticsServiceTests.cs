using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Domain.Entities;
using WiiStudio.Infrastructure.Diagnostics;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Tests.Diagnostics;

public sealed class DiagnosticsServiceTests
{
    [Fact]
    public async Task RunAsync_ShouldReportHealthyWiimmsTools()
    {
        var service = new DiagnosticsService(
            new FakeWitService(new Version(3, 5)),
            new FakeWiimmsToolLocator(wwtPath: "/opt/wiimms/wwt"),
            NullLogger<DiagnosticsService>.Instance);

        var report = await service.RunAsync(CancellationToken.None);

        report.IsHealthy.Should().BeTrue();
        report.Checks.Should().ContainEquivalentOf(new DiagnosticsCheckResult("wit", DiagnosticsStatus.Healthy, "wit is available.", "3.5"));
        report.Checks.Should().ContainEquivalentOf(new DiagnosticsCheckResult("wwt", DiagnosticsStatus.Healthy, "wwt is available."));
    }

    [Fact]
    public async Task RunAsync_ShouldReportUnhealthyWhenWitIsMissing()
    {
        var service = new DiagnosticsService(
            new FakeWitService(new WiimmsNotInstalledException("wit")),
            new FakeWiimmsToolLocator(wwtPath: "/opt/wiimms/wwt"),
            NullLogger<DiagnosticsService>.Instance);

        var report = await service.RunAsync(CancellationToken.None);

        report.IsHealthy.Should().BeFalse();
        report.Checks.Should().ContainEquivalentOf(new DiagnosticsCheckResult("wit", DiagnosticsStatus.Unhealthy, "wit was not found."));
    }

    [Fact]
    public async Task RunAsync_ShouldReportUnhealthyWhenWwtIsMissing()
    {
        var service = new DiagnosticsService(
            new FakeWitService(new Version(3, 5)),
            new FakeWiimmsToolLocator(new WiimmsNotInstalledException("wwt")),
            NullLogger<DiagnosticsService>.Instance);

        var report = await service.RunAsync(CancellationToken.None);

        report.IsHealthy.Should().BeFalse();
        report.Checks.Should().ContainEquivalentOf(new DiagnosticsCheckResult("wwt", DiagnosticsStatus.Unhealthy, "wwt was not found."));
    }

    [Fact]
    public async Task ExportAsync_ShouldWriteJsonReport()
    {
        var path = Path.Combine(Path.GetTempPath(), $"wiistudio-diagnostics-{Guid.NewGuid():N}.json");
        try
        {
            var service = new DiagnosticsService(
                new FakeWitService(new Version(3, 5)),
                new FakeWiimmsToolLocator(wwtPath: "/opt/wiimms/wwt"),
                NullLogger<DiagnosticsService>.Instance);

            await service.ExportAsync(path, CancellationToken.None);

            var json = await File.ReadAllTextAsync(path);
            json.Should().Contain("\"name\": \"wit\"");
            json.Should().Contain("\"status\": 1");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private sealed class FakeWitService : IWitService
    {
        private readonly Version? _version;
        private readonly Exception? _exception;

        public FakeWitService(Version version)
        {
            _version = version;
        }

        public FakeWitService(Exception exception)
        {
            _exception = exception;
        }

        public Task<Version> GetVersionAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(_version!);
        }

        public Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<Game>>([]);
        }
    }

    private sealed class FakeWiimmsToolLocator : IWiimmsToolLocator
    {
        private readonly string? _wwtPath;
        private readonly Exception? _exception;

        public FakeWiimmsToolLocator(string wwtPath)
        {
            _wwtPath = wwtPath;
        }

        public FakeWiimmsToolLocator(Exception exception)
        {
            _exception = exception;
        }

        public Task<string> FindWitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult("/opt/wiimms/wit");
        }

        public Task<string> FindWwtAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(_wwtPath!);
        }
    }
}
