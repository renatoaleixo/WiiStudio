using System.Reflection;
using System.Text;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class ReleaseValidationService : IReleaseValidationService
{
    private static readonly IReadOnlyList<ReleaseValidationChecklistItem> Checklist =
    [
        Gate("build-release", "Build Release aprovado", "Automated", false, "Anexar saida do dotnet build -c Release."),
        Gate("unit-tests", "Testes unitarios aprovados", "Automated", false, "Anexar saida do dotnet test."),
        Gate("integration-fakes", "Testes integracao com fakes aprovados", "Automated", false, "Anexar resultados de integracao com fakes."),
        Gate("linux-smoke", "Smoke test Linux aprovado", "OS", false, "Registrar distro, versao e screenshot da tela principal."),
        Gate("windows-smoke", "Smoke test Windows aprovado", "OS", false, "Registrar Windows 10/11, admin normal/admin e screenshot."),
        Gate("macos-smoke", "Smoke test macOS aprovado", "OS", false, "Registrar versao macOS e permissao de disco."),
        Gate("real-wbfs-hd", "HD WBFS real aprovado", "Hardware", true, "Anexar diagnostico/readiness com lista de jogos."),
        Gate("fat32-wbfs-folder", "FAT32 /wbfs aprovado", "Hardware", true, "Anexar copia segura e teste no Wii."),
        Gate("usb-flash-drive", "Pendrive aprovado", "Hardware", true, "Anexar deteccao, copia de jogo pequeno e teste no loader."),
        Gate("real-wii", "Wii real aprovado", "Console", true, "Anexar loader usado, jogo iniciado e resultado."),
        Gate("linux-polkit", "Polkit Linux aprovado", "Permissions", false, "Registrar com/sem policy e resultado do retry."),
        Gate("windows-admin", "Windows Admin aprovado", "Permissions", false, "Registrar app normal/admin e falha esperada sem permissao."),
        Gate("macos-permissions", "macOS permissoes documentadas/aprovadas", "Permissions", false, "Documentar permissao de disco ou limitacao conhecida."),
        Gate("wiimms-operations", "Wiimms operacoes principais aprovadas", "Tools", false, "Anexar versoes wit/wwt e logs de list/copy/check."),
        Gate("gametdb", "GameTDB aprovado", "Assets", false, "Anexar capa, disc art e metadados baixados."),
        Gate("update-manifest", "Update manifest aprovado", "Release", false, "Anexar manifesto, checksum valido e pacote invalido bloqueado."),
        Gate("packaging", "Packaging aprovado", "Release", false, "Anexar artefatos, SHA256SUMS e smoke test."),
        Gate("destructive-confirmation", "Sem operacao destrutiva sem confirmacao", "Safety", true, "Anexar preview e confirmacao FORMATAR WBFS em dispositivo dedicado.")
    ];

    public IReadOnlyList<ReleaseValidationChecklistItem> GetChecklist()
    {
        return Checklist;
    }

    public async Task<ReleaseValidationPackage> CreateEvidencePackageAsync(
        ReleaseValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Version);

        var root = Path.GetFullPath(request.OutputDirectory);
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "screenshots"));
        Directory.CreateDirectory(Path.Combine(root, "logs"));

        var createdAt = request.CreatedAt ?? DateTimeOffset.UtcNow;
        var files = new List<ReleaseValidationEvidenceFile>();

        await WriteAsync(root, "version.txt", CreateVersionText(request.Version, request.OperatorName, createdAt), "version", files, cancellationToken);
        await WriteAsync(root, "environment-linux.md", CreateEnvironmentText("Linux Mint/Ubuntu", LinuxChecklist()), "environment-linux", files, cancellationToken);
        await WriteAsync(root, "environment-windows.md", CreateEnvironmentText("Windows 10/11", WindowsChecklist()), "environment-windows", files, cancellationToken);
        await WriteAsync(root, "environment-macos.md", CreateEnvironmentText("macOS suportado", MacOsChecklist()), "environment-macos", files, cancellationToken);
        await WriteAsync(root, "hardware-matrix.md", CreateHardwareMatrixText(), "hardware-matrix", files, cancellationToken);
        await WriteAsync(root, "release-checklist.md", CreateChecklistText(Checklist), "release-checklist", files, cancellationToken);

        var acceptanceReport = Path.Combine(root, "acceptance-report.md");
        await File.WriteAllTextAsync(
            acceptanceReport,
            CreateAcceptanceReportText(request.Version, createdAt, Checklist),
            cancellationToken);
        files.Add(new ReleaseValidationEvidenceFile("acceptance-report", acceptanceReport));

        return new ReleaseValidationPackage(
            root,
            acceptanceReport,
            Checklist,
            files,
            IsReleaseApproved: Checklist.All(item => item.Status == ReleaseValidationGateStatus.Passed));
    }

    private static ReleaseValidationChecklistItem Gate(
        string id,
        string title,
        string category,
        bool requiresHardware,
        string evidenceHint)
    {
        return new ReleaseValidationChecklistItem(
            id,
            title,
            category,
            ReleaseValidationGateStatus.Pending,
            requiresHardware,
            evidenceHint);
    }

    private static async Task WriteAsync(
        string root,
        string fileName,
        string content,
        string kind,
        ICollection<ReleaseValidationEvidenceFile> files,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(root, fileName);
        await File.WriteAllTextAsync(path, content, cancellationToken);
        files.Add(new ReleaseValidationEvidenceFile(kind, path));
    }

    private static string CreateVersionText(string version, string? operatorName, DateTimeOffset createdAt)
    {
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
            ?? typeof(ReleaseValidationService).Assembly.GetName().Version?.ToString()
            ?? "unknown";

        return $"""
        WiiStudio release validation
        Requested version: {version}
        Assembly version: {assemblyVersion}
        Generated at: {createdAt:O}
        Operator: {operatorName ?? "pending"}
        Release approved: false

        This package is evidence scaffolding only. Hardware gates remain pending until validated on dedicated devices and a real Wii.
        """;
    }

    private static string CreateEnvironmentText(string title, IReadOnlyList<string> items)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {title}");
        builder.AppendLine();
        foreach (var item in items)
        {
            builder.AppendLine($"- [ ] {item}");
        }

        builder.AppendLine();
        builder.AppendLine("## Evidence");
        builder.AppendLine();
        builder.AppendLine("- Attach screenshots/logs under `screenshots/` and `logs/`.");
        return builder.ToString();
    }

    private static IReadOnlyList<string> LinuxChecklist() =>
    [
        "App starts on Linux Mint/Ubuntu.",
        "Polkit policy installed path recorded.",
        "Behavior without Polkit documented.",
        "Admin and non-admin permission behavior documented.",
        "Wiimms wit/wwt detected and version recorded."
    ];

    private static IReadOnlyList<string> WindowsChecklist() =>
    [
        "App starts on Windows 10/11.",
        "Behavior without Administrator documented.",
        "Behavior as Administrator documented.",
        "Removable disk detection documented.",
        "Permission failure message captured."
    ];

    private static IReadOnlyList<string> MacOsChecklist() =>
    [
        "App starts on supported macOS.",
        "Disk permission prompt or limitation documented.",
        "Intel or Apple Silicon runtime recorded.",
        "Known notarization/signing limitation documented.",
        "USB detection behavior captured."
    ];

    private static string CreateHardwareMatrixText()
    {
        return """
        # Hardware Matrix

        ## USB Devices

        - [ ] External 2.5" USB hard drive.
        - [ ] USB flash drive 16 GB or larger.
        - [ ] Existing WBFS partition.
        - [ ] FAT32 device with `/wbfs`.
        - [ ] NTFS device, when supported by the tested flow.

        ## Wii Console

        - [ ] Real modified Wii.
        - [ ] USB Loader GX or equivalent loader.
        - [ ] SD card, when required.
        - [ ] Stable power and USB cable.

        ## Destructive Test Rule

        Formatting tests must use only dedicated test devices with no important data.
        """;
    }

    private static string CreateChecklistText(IReadOnlyList<ReleaseValidationChecklistItem> checklist)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Release Acceptance Checklist");
        builder.AppendLine();
        foreach (var item in checklist)
        {
            builder.AppendLine($"- [ ] {item.Title} (`{item.Id}`)");
            builder.AppendLine($"  - Category: {item.Category}");
            builder.AppendLine($"  - Requires hardware: {item.RequiresHardware}");
            builder.AppendLine($"  - Evidence: {item.EvidenceHint}");
        }

        return builder.ToString();
    }

    private static string CreateAcceptanceReportText(
        string version,
        DateTimeOffset createdAt,
        IReadOnlyList<ReleaseValidationChecklistItem> checklist)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# WiiStudio Release Acceptance Report");
        builder.AppendLine();
        builder.AppendLine($"Version: {version}");
        builder.AppendLine($"Generated at: {createdAt:O}");
        builder.AppendLine("Release approved: false");
        builder.AppendLine();
        builder.AppendLine("## Release Gate");
        builder.AppendLine();
        builder.AppendLine("This report starts as pending. Do not mark WiiStudio ready for release until every gate below has real evidence.");
        builder.AppendLine();
        builder.AppendLine("## Checklist");
        builder.AppendLine();

        foreach (var item in checklist)
        {
            builder.AppendLine($"- [ ] {item.Title} - {item.Status} - {item.EvidenceHint}");
        }

        builder.AppendLine();
        builder.AppendLine("## Known Risks To Review");
        builder.AppendLine();
        builder.AppendLine("- macOS disk access limitations.");
        builder.AppendLine("- Windows Administrator requirement for selected device operations.");
        builder.AppendLine("- Linux Polkit policy requirement.");
        builder.AppendLine("- WBFS formatting is destructive.");
        builder.AppendLine("- FAT32 `/wbfs` is recommended for broad loader compatibility.");
        builder.AppendLine("- USB device compatibility varies by Wii loader, cable and enclosure.");
        builder.AppendLine();
        builder.AppendLine("## Final Sign-off");
        builder.AppendLine();
        builder.AppendLine("- Operator:");
        builder.AppendLine("- Date:");
        builder.AppendLine("- Decision: Pending");

        return builder.ToString();
    }
}
