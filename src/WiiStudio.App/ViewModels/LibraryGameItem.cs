using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class LibraryGameItem
{
    public LibraryGameItem(GameDto game, string? coverPath = null)
    {
        Game = game;
        CoverPath = coverPath;
        CoverImage = LoadCover(coverPath);
    }

    public GameDto Game { get; }

    public string? CoverPath { get; }

    public bool HasCover => CoverImage is not null;

    public IImage? CoverImage { get; }

    public string Id => Game.Id;

    public string Title => Game.Title;

    public string Region => Game.Region.ToString();

    public string Format => Game.Format.ToString();

    public string Favorite => Game.IsFavorite ? "Favorito" : "";

    public string AddedAt => Game.AddedAt.ToLocalTime().ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);

    public string Size => FormatSize(Game.SizeInBytes);

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
