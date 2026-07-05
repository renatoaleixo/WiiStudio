namespace WiiStudio.Application.DTOs;

public sealed record VerificationReport(
    IReadOnlyList<VerificationResultDto> Results,
    DateTimeOffset VerifiedAt)
{
    public int TotalGames => Results.Count;

    public bool HasProblems => Results.Any(result => result.Status is not VerificationStatus.Healthy);
}
