using FluentValidation;
using WiiStudio.Application.Commands;

namespace WiiStudio.Application.Validators;

public sealed class DownloadCoverCommandValidator : AbstractValidator<DownloadCoverCommand>
{
    public DownloadCoverCommandValidator()
    {
        RuleFor(command => command.GameId)
            .Must(GameIdValidation.IsValid)
            .WithMessage("GameId must contain exactly 6 letters or digits.");
    }
}
