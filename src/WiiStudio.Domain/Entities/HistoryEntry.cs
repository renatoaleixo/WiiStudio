namespace WiiStudio.Domain.Entities;

public sealed class HistoryEntry
{
    public HistoryEntry(string operation, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        Operation = operation.Trim();
        CreatedAt = createdAt;
    }

    public string Operation { get; }

    public DateTimeOffset CreatedAt { get; }
}
