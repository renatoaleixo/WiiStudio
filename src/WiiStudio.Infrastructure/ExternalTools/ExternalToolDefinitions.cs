using WiiStudio.Application.ExternalTools;

namespace WiiStudio.Infrastructure.ExternalTools;

public interface IExternalToolDefinitionProvider
{
    IReadOnlyList<ExternalToolDefinition> GetDefinitions();
}

public sealed class ExternalToolDefinitionProvider : IExternalToolDefinitionProvider
{
    public const string WitToolId = "wiimms-wit";
    public const string WwtToolId = "wiimms-wwt";
    public const string NKitToolId = "nkit";
    public const string DolphinToolId = "dolphin";
    public const string DolphinFlatpakApplicationId = "org.DolphinEmu.dolphin-emu";
    public const string NKitPackageSha256 = "e600b0a2dbacf784779ce33b01259f197c5f8cfc7135f1831160d3f98bb431c6";

    private readonly ExternalToolOptions options;

    public ExternalToolDefinitionProvider(ExternalToolOptions options)
    {
        this.options = options;
    }

    public IReadOnlyList<ExternalToolDefinition> GetDefinitions()
    {
        var toolsRoot = options.ToolsRootDirectory;
        return
        [
            new ExternalToolDefinition(
                WitToolId,
                "Wiimms WIT",
                CandidateNames("wit"),
                [
                    Path.Combine(toolsRoot, "wiimms"),
                    toolsRoot,
                    AppContext.BaseDirectory
                ],
                "--version",
                null,
                IsRequired: true),
            new ExternalToolDefinition(
                WwtToolId,
                "Wiimms WWT",
                CandidateNames("wwt"),
                [
                    Path.Combine(toolsRoot, "wiimms"),
                    toolsRoot,
                    AppContext.BaseDirectory
                ],
                "--version",
                null,
                IsRequired: true),
            new ExternalToolDefinition(
                NKitToolId,
                "NKit Legacy",
                ["RecoverToISO.exe", "ConvertToISO.exe", "NKitProcessingApp.exe"],
                [
                    Path.Combine(toolsRoot, "nkit", "v1.4"),
                    Path.Combine(toolsRoot, "nkit"),
                    toolsRoot,
                    AppContext.BaseDirectory
                ],
                null,
                new ExternalToolPackageSource(
                    "local://tools/packages/NKit_v1.4.zip",
                    NKitPackageSha256,
                    "1.4",
                    "NKit v1 legacy local package",
                    IsTrusted: true,
                    AutoDownloadEnabled: false,
                    LocalPackagePath: Path.Combine(options.PackagesDirectory, "NKit_v1.4.zip")),
                IsRequired: false)
            ,
            new ExternalToolDefinition(
                DolphinToolId,
                "Dolphin Emulator",
                DolphinCandidateNames(),
                DolphinSearchDirectories(toolsRoot),
                "--version",
                null,
                IsRequired: false)
        ];
    }

    private static IReadOnlyList<string> CandidateNames(string toolName)
    {
        return OperatingSystem.IsWindows()
            ? [toolName, $"{toolName}.exe"]
            : [toolName];
    }

    private static IReadOnlyList<string> DolphinCandidateNames()
    {
        if (OperatingSystem.IsWindows())
        {
            return ["Dolphin.exe", "DolphinQt2.exe", "dolphin.exe"];
        }

        if (OperatingSystem.IsMacOS())
        {
            return ["Dolphin", "Dolphin.app"];
        }

        return ["dolphin-emu", "dolphin", "Dolphin", DolphinFlatpakApplicationId];
    }

    private static IReadOnlyList<string> DolphinSearchDirectories(string toolsRoot)
    {
        var directories = new List<string>
        {
            Path.Combine(toolsRoot, "dolphin"),
            toolsRoot,
            AppContext.BaseDirectory
        };

        if (OperatingSystem.IsWindows())
        {
            AddIfNotEmpty(directories, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Dolphin");
            AddIfNotEmpty(directories, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Dolphin");
            AddIfNotEmpty(directories, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dolphin");
        }
        else if (OperatingSystem.IsMacOS())
        {
            directories.Add("/Applications/Dolphin.app/Contents/MacOS");
            directories.Add("/Applications");
        }
        else
        {
            directories.Add("/usr/bin");
            directories.Add("/usr/local/bin");
            directories.Add("/snap/bin");
            directories.Add("/var/lib/flatpak/exports/bin");
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            AddIfNotEmpty(directories, userProfile, ".local/bin");
            AddIfNotEmpty(directories, userProfile, ".local/share/flatpak/exports/bin");
        }

        return directories;
    }

    private static void AddIfNotEmpty(List<string> directories, string root, string relative)
    {
        if (!string.IsNullOrWhiteSpace(root))
        {
            directories.Add(Path.Combine(root, relative));
        }
    }
}
