namespace WiiStudio.Application.Exceptions;

public sealed class UsbDriveNotFoundException : Exception
{
    public UsbDriveNotFoundException(Guid usbDriveId)
        : base($"USB drive '{usbDriveId}' was not found.")
    {
        UsbDriveId = usbDriveId;
    }

    public Guid UsbDriveId { get; }
}
