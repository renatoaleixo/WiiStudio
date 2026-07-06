using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class ImportPreviewItemViewModel
{
    public ImportPreviewItemViewModel(ImportPreviewItem item)
    {
        SourcePath = item.SourcePath;
        Format = item.Format.ToString();
        Status = item.Status.ToString();
        Message = item.Message;
    }

    public string SourcePath { get; }

    public string Format { get; }

    public string Status { get; }

    public string Message { get; }
}
