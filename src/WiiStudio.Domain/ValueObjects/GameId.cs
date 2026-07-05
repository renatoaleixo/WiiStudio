namespace WiiStudio.Domain.ValueObjects;

public readonly record struct GameId
{
    public GameId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length != 6 || normalized.Any(character => !char.IsLetterOrDigit(character)))
        {
            throw new ArgumentException("Game ID must contain exactly 6 letters or digits.", nameof(value));
        }

        Value = normalized;
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(GameId gameId)
    {
        return gameId.Value;
    }
}
