using Avalonia.Media;
using Avalonia.Media.Imaging;
using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class CoverAssetViewModel
{
    public CoverAssetViewModel(CoverAssetKind kind, string title, string? imagePath)
    {
        Kind = kind;
        Title = title;
        ImagePath = imagePath;
        Image = LoadImage(imagePath);
        State = Image is null ? CoverAssetViewState.Missing : CoverAssetViewState.FromCache;
    }

    public CoverAssetKind Kind { get; }

    public string Title { get; }

    public string? ImagePath { get; }

    public IImage? Image { get; }

    public CoverAssetViewState State { get; }

    public bool HasImage => Image is not null;

    public string StateText => State switch
    {
        CoverAssetViewState.FromCache => "Em cache",
        CoverAssetViewState.Available => "Disponível",
        CoverAssetViewState.Missing => "Ausente",
        CoverAssetViewState.Downloading => "Baixando",
        CoverAssetViewState.Failed => "Falha",
        CoverAssetViewState.InvalidImage => "Imagem inválida",
        _ => "Indefinido"
    };

    private static IImage? LoadImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return null;
        }

        try
        {
            return new Bitmap(imagePath);
        }
        catch
        {
            return null;
        }
    }
}
