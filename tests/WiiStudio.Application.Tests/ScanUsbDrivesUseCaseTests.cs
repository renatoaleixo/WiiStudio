using FluentAssertions;
using System.Runtime.CompilerServices;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.UseCases;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests;

public sealed class ScanUsbDrivesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnScannedDrives()
    {
        var expectedDrive = new UsbDriveDto(
            Guid.NewGuid(),
            "/dev/sdb1",
            "/media/wii",
            "Wii HD",
            UsbDriveFileSystem.Wbfs,
            1000,
            600,
            400,
            IsMounted: true,
            IsWritable: true);

        var service = new FakeUsbDetectionService([expectedDrive]);
        var useCase = new ScanUsbDrivesUseCase(service);

        var drives = await useCase.ExecuteAsync(CancellationToken.None);

        drives.Should().ContainSingle().Which.Should().BeSameAs(expectedDrive);
    }

    private sealed class FakeUsbDetectionService : IUsbDetectionService
    {
        private readonly IReadOnlyList<UsbDriveDto> _drives;

        public FakeUsbDetectionService(IReadOnlyList<UsbDriveDto> drives)
        {
            _drives = drives;
        }

        public Task<IReadOnlyList<UsbDriveDto>> ScanAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_drives);
        }

        public async IAsyncEnumerable<UsbDriveDto> WatchAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var drive in _drives)
            {
                yield return drive;
            }

            await Task.CompletedTask;
        }
    }
}
