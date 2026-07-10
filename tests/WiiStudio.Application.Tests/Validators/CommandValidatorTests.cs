using FluentAssertions;
using WiiStudio.Application.Commands;
using WiiStudio.Application.Validators;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Validators;

public sealed class CommandValidatorTests
{
    [Fact]
    public void ImportGameCommandValidator_ShouldRejectEmptySourcePaths()
    {
        var validator = new ImportGameCommandValidator();
        var command = new ImportGameCommand([], Guid.NewGuid(), UserConfirmed: true);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ImportGameCommandValidator_ShouldAcceptValidCommand()
    {
        var validator = new ImportGameCommandValidator();
        var command = new ImportGameCommand(["/games/mario.iso"], Guid.NewGuid(), UserConfirmed: true);

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ImportGameCommandValidator_ShouldRejectUnsupportedExtensions()
    {
        var validator = new ImportGameCommandValidator();
        var command = new ImportGameCommand(["/games/mario.txt"], Guid.NewGuid(), UserConfirmed: true);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void RemoveGameCommandValidator_ShouldRequireConfirmation()
    {
        var validator = new RemoveGameCommandValidator();
        var command = new RemoveGameCommand("RMCP01", Guid.NewGuid(), Confirmed: false);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ConvertGameCommandValidator_ShouldRejectUnknownTargetFormat()
    {
        var validator = new ConvertGameCommandValidator();
        var command = new ConvertGameCommand("/games/source.iso", "/games/output.wbfs", FileFormat.Unknown);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void DownloadCoverCommandValidator_ShouldValidateGameId()
    {
        var validator = new DownloadCoverCommandValidator();
        var command = new DownloadCoverCommand("invalid", ForceRefresh: false);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
