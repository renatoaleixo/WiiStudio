using Microsoft.Extensions.Logging;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Usb;

public sealed class CrossPlatformUsbDeviceProvider : IUsbDeviceProvider
{
    private readonly IOperatingSystemInfo _operatingSystemInfo;
    private readonly IReadOnlyDictionary<OSPlatformKind, IPlatformUsbDeviceProvider> _providers;
    private readonly ILogger<CrossPlatformUsbDeviceProvider> _logger;

    public CrossPlatformUsbDeviceProvider(
        IOperatingSystemInfo operatingSystemInfo,
        IEnumerable<IPlatformUsbDeviceProvider> providers,
        ILogger<CrossPlatformUsbDeviceProvider> logger)
    {
        _operatingSystemInfo = operatingSystemInfo;
        _providers = providers.ToDictionary(provider => provider.Platform);
        _logger = logger;
    }

    public async Task<IReadOnlyList<UsbDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_providers.TryGetValue(_operatingSystemInfo.Current, out var provider))
        {
            _logger.LogWarning("No USB device provider registered for platform {Platform}", _operatingSystemInfo.Current);
            return [];
        }

        try
        {
            return await provider.GetDevicesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "USB device provider {Provider} failed", provider.GetType().Name);
            return [];
        }
    }
}
