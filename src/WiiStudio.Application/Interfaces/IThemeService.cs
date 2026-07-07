using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IThemeService
{
    Task<IReadOnlyList<ThemeInfo>> GetThemesAsync(CancellationToken cancellationToken);

    Task<ThemeSnapshot> ApplyThemeAsync(string themeId, CancellationToken cancellationToken);

    Task<ThemeInfo> ImportThemeAsync(string filePath, CancellationToken cancellationToken);

    Task ExportThemeAsync(string themeId, string filePath, CancellationToken cancellationToken);
}
