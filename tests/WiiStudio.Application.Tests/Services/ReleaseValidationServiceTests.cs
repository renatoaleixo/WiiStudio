using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class ReleaseValidationServiceTests : IDisposable
{
    private readonly string tempRoot = Path.Combine(Path.GetTempPath(), "wiistudio-release-validation-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task CreateEvidencePackageAsync_ShouldCreateRequiredEvidenceStructure()
    {
        var service = new ReleaseValidationService();
        var output = Path.Combine(tempRoot, "release-validation");

        var package = await service.CreateEvidencePackageAsync(
            new ReleaseValidationRequest(output, "1.0.0", "qa", new DateTimeOffset(2026, 7, 10, 12, 0, 0, TimeSpan.Zero)),
            CancellationToken.None);

        package.IsReleaseApproved.Should().BeFalse();
        package.RootDirectory.Should().Be(Path.GetFullPath(output));
        File.Exists(Path.Combine(output, "version.txt")).Should().BeTrue();
        File.Exists(Path.Combine(output, "environment-linux.md")).Should().BeTrue();
        File.Exists(Path.Combine(output, "environment-windows.md")).Should().BeTrue();
        File.Exists(Path.Combine(output, "environment-macos.md")).Should().BeTrue();
        File.Exists(Path.Combine(output, "hardware-matrix.md")).Should().BeTrue();
        File.Exists(Path.Combine(output, "release-checklist.md")).Should().BeTrue();
        File.Exists(Path.Combine(output, "acceptance-report.md")).Should().BeTrue();
        Directory.Exists(Path.Combine(output, "screenshots")).Should().BeTrue();
        Directory.Exists(Path.Combine(output, "logs")).Should().BeTrue();
    }

    [Fact]
    public async Task CreateEvidencePackageAsync_ShouldKeepHardwareGatesPendingUntilManualEvidenceExists()
    {
        var service = new ReleaseValidationService();

        var package = await service.CreateEvidencePackageAsync(
            new ReleaseValidationRequest(Path.Combine(tempRoot, "release-validation"), "1.0.0"),
            CancellationToken.None);

        package.Checklist.Should().Contain(item =>
            item.Id == "real-wbfs-hd" &&
            item.RequiresHardware &&
            item.Status == ReleaseValidationGateStatus.Pending);
        package.Checklist.Should().Contain(item =>
            item.Id == "real-wii" &&
            item.RequiresHardware &&
            item.Status == ReleaseValidationGateStatus.Pending);
        File.ReadAllText(package.AcceptanceReportPath)
            .Should()
            .Contain("Do not mark WiiStudio ready for release until every gate below has real evidence.");
    }

    [Fact]
    public void GetChecklist_ShouldCoverCriticalReleaseAcceptanceItems()
    {
        var service = new ReleaseValidationService();

        var checklist = service.GetChecklist();

        checklist.Select(item => item.Id).Should().Contain([
            "build-release",
            "unit-tests",
            "real-wbfs-hd",
            "fat32-wbfs-folder",
            "usb-flash-drive",
            "real-wii",
            "linux-polkit",
            "update-manifest",
            "packaging",
            "destructive-confirmation"
        ]);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
