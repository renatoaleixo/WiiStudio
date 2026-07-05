using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Mappers;

namespace WiiStudio.Infrastructure.Usb;

public sealed class UsbDetectionService : IUsbDetectionService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(2);

    private readonly IUsbProvider _usbProvider;
    private readonly ILogger<UsbDetectionService> _logger;

    public UsbDetectionService(IUsbProvider usbProvider, ILogger<UsbDetectionService> logger)
    {
        _usbProvider = usbProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UsbDriveDto>> ScanAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting USB scan");

        var drives = await _usbProvider.DetectAsync(cancellationToken);
        var result = drives.Select(UsbDriveMapper.ToDto).ToArray();

        _logger.LogInformation("USB scan completed with {DriveCount} compatible drive(s)", result.Length);

        return result;
    }

    public async IAsyncEnumerable<UsbDriveDto> WatchAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var knownDriveIds = new HashSet<Guid>();

        while (!cancellationToken.IsCancellationRequested)
        {
            var drives = await ScanAsync(cancellationToken);

            foreach (var drive in drives.Where(drive => knownDriveIds.Add(drive.Id)))
            {
                yield return drive;
            }

            await Task.Delay(PollingInterval, cancellationToken);
        }
    }
}
