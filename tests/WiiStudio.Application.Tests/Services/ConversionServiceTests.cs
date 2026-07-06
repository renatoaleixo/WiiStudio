using FluentAssertions;
using FluentValidation;
using WiiStudio.Application.Commands;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Application.Validators;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class ConversionServiceTests
{
    [Fact]
    public async Task ConvertAsync_ShouldCopyUsingWitService()
    {
        var witService = new FakeWitService();
        var service = new ConversionService(new ConvertGameCommandValidator(), witService);

        var job = await service.ConvertAsync(
            new ConvertGameCommand("/games/mario.iso", "/converted/mario.wbfs", FileFormat.Wbfs),
            CancellationToken.None);

        job.Status.Should().Be(ConversionJobStatus.Completed);
        job.SourceFormat.Should().Be(FileFormat.Iso);
        witService.CopiedGames.Should().ContainSingle().Which.Should().Be(("/games/mario.iso", "/converted/mario.wbfs", FileFormat.Wbfs));
    }

    [Fact]
    public async Task ConvertAsync_ShouldReturnFailedJobWhenProviderFails()
    {
        var service = new ConversionService(new ConvertGameCommandValidator(), new FakeWitService(throwOnCopy: true));

        var job = await service.ConvertAsync(
            new ConvertGameCommand("/games/mario.iso", "/converted/mario.wbfs", FileFormat.Wbfs),
            CancellationToken.None);

        job.Status.Should().Be(ConversionJobStatus.Failed);
        job.ErrorMessage.Should().Be("copy failed");
    }

    [Fact]
    public async Task ConvertAsync_ShouldThrowWhenCommandIsInvalid()
    {
        var service = new ConversionService(new ConvertGameCommandValidator(), new FakeWitService());

        var act = () => service.ConvertAsync(new ConvertGameCommand("", "/converted/mario.wbfs", FileFormat.Wbfs), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private sealed class FakeWitService : IWitService
    {
        private readonly bool _throwOnCopy;

        public FakeWitService(bool throwOnCopy = false)
        {
            _throwOnCopy = throwOnCopy;
        }

        public List<(string SourcePath, string DestinationPath, FileFormat TargetFormat)> CopiedGames { get; } = [];

        public Task<Version> GetVersionAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new Version(1, 0));
        }

        public Task<IReadOnlyList<Game>> ListGamesAsync(UsbDrive drive, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<Game>>([]);
        }

        public Task VerifyAsync(string sourcePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }

        public Task CopyAsync(string sourcePath, string destinationPath, FileFormat targetFormat, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_throwOnCopy)
            {
                throw new InvalidOperationException("copy failed");
            }

            CopiedGames.Add((sourcePath, destinationPath, targetFormat));
            return Task.CompletedTask;
        }
    }
}
