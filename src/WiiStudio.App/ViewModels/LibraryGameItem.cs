using System.Globalization;
using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class LibraryGameItem
{
    public LibraryGameItem(GameDto game)
    {
        Game = game;
    }

    public GameDto Game { get; }

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
}
