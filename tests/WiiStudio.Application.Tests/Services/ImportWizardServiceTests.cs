using FluentAssertions;
using FluentValidation;
using WiiStudio.Application.Commands;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Application.UseCases;
using WiiStudio.Application.Validators;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class ImportWizardServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_ShouldDetectSupportedFormats()
    {
        var service = CreateService(CreateDrive());
        using var fixture = TemporaryImportFiles.Create(["mario.iso", "zelda.wbfs", "metroid.rvz", "kirby.nkit.iso"]);

        var preview = await service.AnalyzeAsync(
            fixture.Paths,
            CancellationToken.None);

        preview.CanImport.Should().BeTrue();
        preview.State.Should().Be(ImportWizardState.SelectingTarget);
        preview.Items.Select(item => item.Format).Should().Equal(
            FileFormat.Iso,
            FileFormat.Wbfs,
            FileFormat.Rvz,
            FileFormat.NKit);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldRejectUnsupportedFormats()
    {
        var service = CreateService(CreateDrive());
        using var fixture = TemporaryImportFiles.Create(["readme.txt"]);

        var preview = await service.AnalyzeAsync(fixture.Paths, CancellationToken.None);

        preview.CanImport.Should().BeFalse();
        preview.State.Should().Be(ImportWizardState.Validating);
        preview.Items.Should().ContainSingle().Which.Status.Should().Be(ImportWizardFileStatus.Error);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldRejectMissingFile()
    {
        var service = CreateService(CreateDrive());

        var preview = await service.AnalyzeAsync(["/games/missing.iso"], CancellationToken.None);

        preview.CanImport.Should().BeFalse();
        preview.Items.Should().ContainSingle().Which.Message.Should().Be("Arquivo não encontrado.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDelegateToImportUseCase()
    {
        var drive = CreateDrive();
        var workflowService = new FakeUsbWriteWorkflowService();
        var service = CreateService(drive, workflowService);

        var result = await service.ExecuteAsync(
            new ImportWizardRequest(["/games/mario.iso", "/games/zelda.wbfs"], drive.Id, UserConfirmed: true),
            CancellationToken.None);

        result.State.Should().Be(ImportWizardState.Completed);
        result.ImportedCount.Should().Be(2);
        workflowService.ExecutedSources.Should().Equal("/games/mario.iso", "/games/zelda.wbfs");
    }

    [Theory]
    [InlineData("/games/metroid.rvz")]
    [InlineData("/games/kirby.nkit.iso")]
    public async Task ImportGameCommandValidator_ShouldAcceptWizardFormats(string sourcePath)
    {
        var validator = new ImportGameCommandValidator();

        var result = await validator.ValidateAsync(new ImportGameCommand([sourcePath], Guid.NewGuid(), UserConfirmed: true));

        result.IsValid.Should().BeTrue();
    }

    private static ImportWizardService CreateService(UsbDrive drive, FakeUsbWriteWorkflowService? workflowService = null)
    {
        var useCase = new ImportGamesUseCase(
            new ImportGameCommandValidator(),
            new FakeUsbDriveService([drive]),
            workflowService ?? new FakeUsbWriteWorkflowService(),
            new FakeGameLibraryService());

        return new ImportWizardService(useCase);
    }

    private static UsbDrive CreateDrive()
    {
        return new UsbDrive(Guid.NewGuid(), "/dev/sdb1", "/media/wii", "Wii HD", UsbDriveFileSystem.Wbfs, 1000, 500, 500, true, true);
    }

    private sealed class TemporaryImportFiles : IDisposable
    {
        private TemporaryImportFiles(string rootDirectory, IReadOnlyList<string> paths)
        {
            RootDirectory = rootDirectory;
            Paths = paths;
        }

        public string RootDirectory { get; }

        public IReadOnlyList<string> Paths { get; }

        public static TemporaryImportFiles Create(IReadOnlyList<string> fileNames)
        {
            var rootDirectory = Path.Combine(Path.GetTempPath(), $"wiistudio-import-{Guid.NewGuid():N}");
            Directory.CreateDirectory(rootDirectory);
            var paths = fileNames
                .Select(fileName => Path.Combine(rootDirectory, fileName))
                .ToArray();

            foreach (var path in paths)
            {
                File.WriteAllText(path, "game");
            }

            return new TemporaryImportFiles(rootDirectory, paths);
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }
    }

    private sealed class FakeUsbDriveService : IUsbDriveService
    {
        private readonly IReadOnlyList<UsbDrive> _drives;

        public FakeUsbDriveService(IReadOnlyList<UsbDrive> drives)
        {
            _drives = drives;
        }

        public Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_drives);
        }
    }

    private sealed class FakeUsbWriteWorkflowService : IUsbWriteWorkflowService
    {
        public List<string> ExecutedSources { get; } = [];

        public Task<UsbWritePreview> CreatePreviewAsync(string sourcePath, Guid targetDriveId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new UsbWritePreview(
                sourcePath,
                GameId: null,
                targetDriveId,
                "Wii HD",
                "/dev/sdb1",
                "/media/wii",
                UsbDriveFileSystem.Wbfs,
                SourceSizeBytes: 100,
                RequiredBytes: 100,
                FreeBytes: 500,
                RequiresFat32Split: false,
                UserConfirmationRequired: true,
                IsBlocked: false,
                BlockReasonKey: null,
                PermissionStatus: null,
                Warnings: []));
        }

        public Task<UsbWriteResult> ExecuteAsync(UsbWriteExecutionRequest request, IProgress<WiimmsOperationProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutedSources.Add(request.Preview.SourcePath);
            return Task.FromResult(new UsbWriteResult(true, request.Preview.GameId, request.Preview.TargetDriveId, null, TimeSpan.FromMilliseconds(1)));
        }
    }

    private sealed class FakeGameLibraryService : IGameLibraryService
    {
        public Task<IReadOnlyList<GameDto>> GetGamesAsync(Guid? usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<GameDto>>([]);
        }

        public Task RefreshAsync(Guid usbDriveId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }
}
