using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class DiagnosticsCheckItemViewModel
{
    public DiagnosticsCheckItemViewModel(DiagnosticsCheckResult check)
    {
        Name = check.Name;
        Status = check.Status.ToString();
        Category = check.Category;
        Severity = check.Severity.ToString();
        Message = check.Message;
        Detail = string.IsNullOrWhiteSpace(check.Version) ? check.Message : check.Version;
    }

    public string Name { get; }

    public string Status { get; }

    public string Category { get; }

    public string Severity { get; }

    public string Message { get; }

    public string Detail { get; }
}
