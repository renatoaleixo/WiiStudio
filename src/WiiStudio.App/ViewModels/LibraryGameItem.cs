using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed partial class LibraryGameItem : ObservableObject
{
    public LibraryGameItem(GameDto game, string? coverPath = null, GameMetadataDto? metadata = null)
    {
        Game = game;
        Metadata = metadata;
        CoverPath = coverPath;
        CoverImage = LoadCover(coverPath);
    }

    public GameDto Game { get; private set; }

    public GameMetadataDto? Metadata { get; }

    public string? CoverPath { get; }

    public bool HasCover => CoverImage is not null;

    public IImage? CoverImage { get; }

    public bool HasMetadata => Metadata is not null;

    public bool IsFavorite => Game.IsFavorite;

    [ObservableProperty]
    private bool _isSelected;

    public string Id => Game.Id;

    public string Title => string.IsNullOrWhiteSpace(Metadata?.Title) ? Game.Title : Metadata.Title;

    public string MetadataSummary
    {
        get
        {
            var parts = new[]
            {
                Metadata?.Year?.ToString(CultureInfo.CurrentCulture),
                Metadata?.Developer,
                Metadata?.Publisher
            }.Where(part => !string.IsNullOrWhiteSpace(part));

            var summary = string.Join(" - ", parts);
            return string.IsNullOrWhiteSpace(summary) ? "Metadados pendentes" : summary;
        }
    }

    public string Region => Game.Region.ToString();

    public string Format => Game.Format.ToString();

    public string Favorite => Game.IsFavorite ? "Favorito" : "";

    public string CoverStatusText => HasCover ? "Capa disponível" : "Capa pendente";

    public string MetadataStatusText => HasMetadata ? "Metadados sincronizados" : "Metadados pendentes";

    public string FavoriteStatusText => IsFavorite ? "Favorito" : "Não favorito";

    public string FavoriteActionText => IsFavorite ? "Remover dos favoritos" : "Marcar como favorito";

    public string Developer => ValueOrFallback(Metadata?.Developer);

    public string Publisher => ValueOrFallback(Metadata?.Publisher);

    public string Year => Metadata?.Year?.ToString(CultureInfo.CurrentCulture) ?? "-";

    public string Genres => FormatList(Metadata?.Genres);

    public string Languages => FormatList(Metadata?.Languages);

    public string Players => Metadata?.Players?.ToString(CultureInfo.CurrentCulture) ?? "-";

    public string Rating => ValueOrFallback(Metadata?.Rating);

    public string Description => string.IsNullOrWhiteSpace(Metadata?.Description)
        ? "Nenhuma descrição sincronizada."
        : Metadata.Description;

    public string MetadataUpdatedAt => Metadata?.UpdatedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture) ?? "-";

    public string AddedAt => Game.AddedAt.ToLocalTime().ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);

    private static string ValueOrFallback(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string FormatList(IReadOnlyList<string>? values)
    {
        if (values is null || values.Count == 0)
        {
            return "-";
        }

        var formatted = string.Join(", ", values.Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(formatted) ? "-" : formatted;
    }

    public string Size => FormatSize(Game.SizeInBytes);

    public void SetFavorite(bool isFavorite)
    {
        if (Game.IsFavorite == isFavorite)
        {
            return;
        }

        Game = Game with { IsFavorite = isFavorite };
        OnPropertyChanged(nameof(IsFavorite));
        OnPropertyChanged(nameof(Favorite));
        OnPropertyChanged(nameof(FavoriteStatusText));
        OnPropertyChanged(nameof(FavoriteActionText));
    }

    private static string FormatSize(long sizeInBytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var size = (double)sizeInBytes;
        var unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }

    private static IImage? LoadCover(string? coverPath)
    {
        if (string.IsNullOrWhiteSpace(coverPath) || !File.Exists(coverPath))
        {
            return null;
        }

        try
        {
            return new Bitmap(coverPath);
        }
        catch
        {
            return null;
        }
    }
}
