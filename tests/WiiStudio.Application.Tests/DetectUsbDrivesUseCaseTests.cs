using FluentAssertions;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.UseCases;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests;

public sealed class DetectUsbDrivesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnDetectedDrives()
    {
        var expectedDrive = new UsbDrive("Wii HD", "/media/wii", UsbDriveFileSystem.Wbfs, 1000, 500);
        var service = new FakeUsbDriveService([expectedDrive]);
        var useCase = new DetectUsbDrivesUseCase(service);

        var drives = await useCase.ExecuteAsync(CancellationToken.None);

        drives.Should().ContainSingle().Which.Should().BeSameAs(expectedDrive);
    }

    private sealed class FakeUsbDriveService : IUsbDriveService
    {
        private readonly IReadOnlyList<UsbDrive> _drives;

        public FakeUsbDriveService(IReadOnlyList<UsbDrive> drives)
        {
            _drives = drives;
        }

        public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_drives);
        }
    }
}
