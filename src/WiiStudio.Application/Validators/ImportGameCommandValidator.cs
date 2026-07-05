using FluentValidation;
using WiiStudio.Application.Commands;

namespace WiiStudio.Application.Validators;

public sealed class ImportGameCommandValidator : AbstractValidator<ImportGameCommand>
{
    public ImportGameCommandValidator()
    {
        RuleFor(command => command.SourcePaths)
            .NotEmpty();

        RuleForEach(command => command.SourcePaths)
            .NotEmpty();

        RuleFor(command => command.DestinationDriveId)
            .NotEmpty();
    }
}
