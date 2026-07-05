namespace WiiStudio.Application.Validators;

public static class GameIdValidation
{
    public static string Normalize(string value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException("GameId must contain exactly 6 letters or digits.", nameof(value));
        }

        return value.Trim().ToUpperInvariant();
    }

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        return trimmed.Length == 6 && trimmed.All(char.IsLetterOrDigit);
    }
}
