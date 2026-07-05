namespace WiiStudio.Application.Validators;

internal static class GameIdValidation
{
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
