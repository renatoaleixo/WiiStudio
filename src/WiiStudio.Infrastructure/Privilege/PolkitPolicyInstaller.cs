using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class PolkitPolicyInstaller : IPolkitPolicyInstaller
{
    private static readonly TimeSpan InstallTimeout = TimeSpan.FromMinutes(2);
    private const string OfficialPolicyXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <policyconfig>
          <vendor>WiiStudio</vendor>
          <action id="com.wiistudio.device.raw-access">
            <description>Allow WiiStudio to access Wii USB/WBFS devices</description>
            <message>Authentication is required to access Wii USB/WBFS devices</message>
            <defaults>
              <allow_any>auth_admin</allow_any>
              <allow_inactive>auth_admin</allow_inactive>
              <allow_active>auth_admin_keep</allow_active>
            </defaults>
          </action>
        </policyconfig>
        """;

    private readonly IOperatingSystemInfo _operatingSystemInfo;
    private readonly IPolkitPolicyLocator _policyLocator;
    private readonly IPolkitPolicyValidator _policyValidator;
    private readonly IProcessRunner _processRunner;

    public PolkitPolicyInstaller(
        IOperatingSystemInfo operatingSystemInfo,
        IPolkitPolicyLocator policyLocator,
        IPolkitPolicyValidator policyValidator,
        IProcessRunner processRunner)
    {
        _operatingSystemInfo = operatingSystemInfo;
        _policyLocator = policyLocator;
        _policyValidator = policyValidator;
        _processRunner = processRunner;
    }

    public async Task<PolkitPolicyInstallStatus> CheckAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_operatingSystemInfo.Current != OSPlatformKind.Linux)
        {
            return Status(PolkitPolicyInstallState.NotLinux, "Polkit é necessário apenas no Linux.", "Nenhuma ação necessária.");
        }

        if (FindExecutable(PolkitPolicyConstants.PkexecExecutable) is null)
        {
            return Status(
                PolkitPolicyInstallState.PkexecMissing,
                "O comando pkexec não foi encontrado.",
                "Instale o Polkit/pkexec pelo gerenciador de pacotes da distribuição.");
        }

        if (!await _policyLocator.ExistsAsync(cancellationToken))
        {
            return Status(
                PolkitPolicyInstallState.Missing,
                "A policy Polkit do WiiStudio não está instalada.",
                $"Instale a policy oficial em {_policyLocator.ExpectedPolicyPath}.");
        }

        var validation = await ValidateInstalledPolicyAsync(cancellationToken);
        return validation.IsValid
            ? Status(PolkitPolicyInstallState.Installed, "Policy Polkit instalada e válida.", "Nenhuma ação necessária.", validation)
            : Status(PolkitPolicyInstallState.Invalid, "A policy Polkit instalada é inválida.", "Reinstale a policy oficial do WiiStudio.", validation);
    }

    public async Task<PolkitPolicyInstallResult> InstallAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var status = await CheckAsync(cancellationToken);
        if (status.State == PolkitPolicyInstallState.Installed)
        {
            return Result(status.State, success: true, status.MessageKey, status.RecommendedActionKey, status.DiagnosticDetails);
        }

        if (_operatingSystemInfo.Current != OSPlatformKind.Linux)
        {
            return Result(PolkitPolicyInstallState.NotLinux, success: false, status.MessageKey, status.RecommendedActionKey, status.DiagnosticDetails);
        }

        var pkexecPath = FindExecutable(PolkitPolicyConstants.PkexecExecutable);
        if (pkexecPath is null)
        {
            return Result(PolkitPolicyInstallState.PkexecMissing, success: false, status.MessageKey, status.RecommendedActionKey, status.DiagnosticDetails);
        }

        var process = File.Exists(PolkitPolicyConstants.InstallerPath)
            ? await RunPackagedInstallerAsync(pkexecPath, cancellationToken)
            : await RunInlinePolicyInstallAsync(pkexecPath, cancellationToken);

        if (!process.IsSuccess)
        {
            return Result(
                PolkitPolicyInstallState.InstallFailed,
                success: false,
                "Falha ao instalar a policy Polkit.",
                "Autorize o prompt do sistema e tente novamente.",
                process.StandardError);
        }

        var validation = await ValidateInstalledPolicyAsync(cancellationToken);
        return validation.IsValid
            ? Result(PolkitPolicyInstallState.Installed, success: true, "Policy instalada com sucesso.", "Tente acessar o dispositivo novamente.", string.Join("; ", validation.Errors))
            : Result(PolkitPolicyInstallState.ValidationFailed, success: false, "A policy instalada não passou na validação.", "Reinstale a policy oficial.", string.Join("; ", validation.Errors));
    }

    public Task<PolkitPolicyValidationResult> ValidateInstalledPolicyAsync(CancellationToken cancellationToken = default)
    {
        return _policyValidator.ValidateInstalledPolicyAsync(_policyLocator.ExpectedPolicyPath, cancellationToken);
    }

    private Task<ProcessResult> RunPackagedInstallerAsync(string pkexecPath, CancellationToken cancellationToken)
    {
        return _processRunner.RunAsync(
            new ProcessRequest(pkexecPath, [PolkitPolicyConstants.InstallerPath, PolkitPolicyConstants.InstallCommand], InstallTimeout),
            cancellationToken);
    }

    private async Task<ProcessResult> RunInlinePolicyInstallAsync(string pkexecPath, CancellationToken cancellationToken)
    {
        var installPath = FindExecutable("install")
            ?? throw new FileNotFoundException("The install command was not found in PATH.", "install");
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"wiistudio-polkit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        var sourcePolicy = Path.Combine(tempDirectory, Path.GetFileName(_policyLocator.ExpectedPolicyPath));
        try
        {
            await File.WriteAllTextAsync(sourcePolicy, OfficialPolicyXml, cancellationToken);
            var sourceValidation = await _policyValidator.ValidateSourcePolicyAsync(sourcePolicy, cancellationToken);
            if (!sourceValidation.IsValid)
            {
                return new ProcessResult(
                    125,
                    string.Empty,
                    $"Generated policy failed validation: {string.Join("; ", sourceValidation.Errors)}",
                    TimeSpan.Zero);
            }

            return await _processRunner.RunAsync(
                new ProcessRequest(
                    pkexecPath,
                    [
                        installPath,
                        "-o",
                        "root",
                        "-g",
                        "root",
                        "-m",
                        "0644",
                        sourcePolicy,
                        _policyLocator.ExpectedPolicyPath
                    ],
                    InstallTimeout),
                cancellationToken);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private PolkitPolicyInstallStatus Status(
        PolkitPolicyInstallState state,
        string message,
        string action,
        PolkitPolicyValidationResult? validation = null)
    {
        return new PolkitPolicyInstallStatus(
            state,
            _policyLocator.ExpectedPolicyPath,
            message,
            action,
            validation is null ? null : string.Join("; ", validation.Errors));
    }

    private PolkitPolicyInstallResult Result(
        PolkitPolicyInstallState state,
        bool success,
        string? message,
        string? action,
        string? details)
    {
        return new PolkitPolicyInstallResult(state, success, _policyLocator.ExpectedPolicyPath, message, action, details);
    }

    private static string? FindExecutable(string executableName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return path
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(directory => Path.Combine(directory, executableName))
            .FirstOrDefault(File.Exists);
    }
}
