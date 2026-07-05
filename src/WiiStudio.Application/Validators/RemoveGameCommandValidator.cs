using FluentValidation;
using WiiStudio.Application.Commands;

namespace WiiStudio.Application.Validators;

public sealed class RemoveGameCommandValidator : AbstractValidator<RemoveGameCommand>
{
    public RemoveGameCommandValidator()
    {
        RuleFor(command => command.GameId)
            .Must(GameIdValidation.IsValid)
            .WithMessage("GameId must contain exactly 6 letters or digits.");

        RuleFor(command => command.SourceDriveId)
            .NotEmpty();

        RuleFor(command => command.Confirmed)
            .Equal(true)
            .WithMessage("Removing a game requires explicit confirmation.");
    }
}
