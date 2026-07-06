using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class ConversionPreviewItemViewModel
{
    public ConversionPreviewItemViewModel(ConversionPreviewItem item)
    {
        SourcePath = item.SourcePath;
        DestinationPath = item.DestinationPath;
        SourceFormat = item.SourceFormat.ToString();
        TargetFormat = item.TargetFormat.ToString();
        Pipeline = item.Pipeline;
        Message = item.Message;
    }

    public string SourcePath { get; }

    public string DestinationPath { get; }

    public string SourceFormat { get; }

    public string TargetFormat { get; }

    public string Pipeline { get; }

    public string Message { get; }
}
