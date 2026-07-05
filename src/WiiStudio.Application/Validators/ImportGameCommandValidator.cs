using FluentValidation;
using WiiStudio.Application.Commands;

namespace WiiStudio.Application.Validators;

public sealed class ImportGameCommandValidator : AbstractValidator<ImportGameCommand>
{
    private static readonly string[] SupportedExtensions = [".iso", ".wbfs"];

    public ImportGameCommandValidator()
    {
        RuleFor(command => command.SourcePaths)
            .NotEmpty();

        RuleForEach(command => command.SourcePaths)
            .NotEmpty()
            .Must(HaveSupportedExtension)
            .WithMessage("Source path must use a supported extension: .iso or .wbfs.");

        RuleFor(command => command.DestinationDriveId)
            .NotEmpty();
    }

    private static bool HaveSupportedExtension(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return false;
        }

        var extension = Path.GetExtension(sourcePath);

        return SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }
}
