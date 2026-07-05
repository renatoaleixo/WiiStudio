using FluentValidation;
using WiiStudio.Application.Commands;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Validators;

public sealed class ConvertGameCommandValidator : AbstractValidator<ConvertGameCommand>
{
    public ConvertGameCommandValidator()
    {
        RuleFor(command => command.SourcePath)
            .NotEmpty();

        RuleFor(command => command.DestinationPath)
            .NotEmpty();

        RuleFor(command => command.TargetFormat)
            .Must(format => format != FileFormat.Unknown)
            .WithMessage("Target format must be supported.");
    }
}
