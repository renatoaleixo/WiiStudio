using WiiStudio.Domain.Enums;

namespace WiiStudio.Domain.Entities;

public sealed class ConversionJob
{
    public ConversionJob(Guid id, string sourcePath, string destinationPath, FileFormat sourceFormat, FileFormat targetFormat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

        if (id == Guid.Empty)
        {
            throw new ArgumentException("Conversion job id cannot be empty.", nameof(id));
        }

        Id = id;
        SourcePath = sourcePath.Trim();
        DestinationPath = destinationPath.Trim();
        SourceFormat = sourceFormat;
        TargetFormat = targetFormat;
        Status = ConversionJobStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }

    public string SourcePath { get; }

    public string DestinationPath { get; }

    public FileFormat SourceFormat { get; }

    public FileFormat TargetFormat { get; }

    public ConversionJobStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? FinishedAt { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void Start()
    {
        EnsureStatus(ConversionJobStatus.Pending);

        Status = ConversionJobStatus.Running;
    }

    public void Complete()
    {
        EnsureStatus(ConversionJobStatus.Running);

        Status = ConversionJobStatus.Completed;
        FinishedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        EnsureStatus(ConversionJobStatus.Running);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        Status = ConversionJobStatus.Failed;
        ErrorMessage = errorMessage.Trim();
        FinishedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status is not (ConversionJobStatus.Pending or ConversionJobStatus.Running))
        {
            throw new InvalidOperationException($"Cannot cancel conversion job in status {Status}.");
        }

        Status = ConversionJobStatus.Cancelled;
        FinishedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureStatus(ConversionJobStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidOperationException($"Expected status {expectedStatus}, but current status is {Status}.");
        }
    }
}
