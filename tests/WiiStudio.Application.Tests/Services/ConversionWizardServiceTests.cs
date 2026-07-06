using FluentAssertions;
using WiiStudio.Application.Commands;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class ConversionWizardServiceTests
{
    [Theory]
    [InlineData("/games/mario.iso", FileFormat.Wbfs, "Wiimms ISO -> WBFS")]
    [InlineData("/games/zelda.wbfs", FileFormat.Iso, "Wiimms WBFS -> ISO")]
    [InlineData("/games/metroid.rvz", FileFormat.Wbfs, "RVZ Provider -> ISO -> Wiimms")]
    [InlineData("/games/kirby.nkit.iso", FileFormat.Iso, "NKit -> ISO")]
    public async Task AnalyzeAsync_ShouldResolveSupportedPipelines(string sourcePath, FileFormat targetFormat, string pipeline)
    {
        var service = new ConversionWizardService(new FakeConversionService());

        var preview = await service.AnalyzeAsync(
            new ConversionWizardRequest([sourcePath], "/converted", targetFormat),
            CancellationToken.None);

        preview.CanConvert.Should().BeTrue();
        preview.State.Should().Be(ConversionWizardState.Reviewing);
        preview.Items.Should().ContainSingle().Which.Pipeline.Should().Be(pipeline);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldRejectSameSourceAndTargetFormat()
    {
        var service = new ConversionWizardService(new FakeConversionService());

        var preview = await service.AnalyzeAsync(
            new ConversionWizardRequest(["/games/mario.iso"], "/converted", FileFormat.Iso),
            CancellationToken.None);

        preview.CanConvert.Should().BeFalse();
        preview.Items.Should().ContainSingle().Which.Message.Should().Be("Origem e destino usam o mesmo formato.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldConvertEveryPreviewItem()
    {
        var conversionService = new FakeConversionService();
        var service = new ConversionWizardService(conversionService);

        var result = await service.ExecuteAsync(
            new ConversionWizardRequest(["/games/mario.iso", "/games/zelda.iso"], "/converted", FileFormat.Wbfs),
            CancellationToken.None);

        result.State.Should().Be(ConversionWizardState.Completed);
        result.Jobs.Should().HaveCount(2);
        conversionService.Commands.Select(command => command.DestinationPath).Should().Equal(
            "/converted/mario.wbfs",
            "/converted/zelda.wbfs");
    }

    private sealed class FakeConversionService : IConversionService
    {
        public List<ConvertGameCommand> Commands { get; } = [];

        public Task<ConversionJobDto> ConvertAsync(ConvertGameCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Commands.Add(command);

            return Task.FromResult(new ConversionJobDto(
                Guid.NewGuid(),
                command.SourcePath,
                command.DestinationPath,
                FileFormat.Iso,
                command.TargetFormat,
                ConversionJobStatus.Completed,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                ErrorMessage: null));
        }
    }
}
