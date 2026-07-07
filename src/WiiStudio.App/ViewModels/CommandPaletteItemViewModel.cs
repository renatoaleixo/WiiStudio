using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed class CommandPaletteItemViewModel
{
    public CommandPaletteItemViewModel(CommandItem command)
    {
        Id = command.Id;
        Title = command.Title;
        Category = command.Category.ToString();
        Description = command.Description;
    }

    public string Id { get; }

    public string Title { get; }

    public string Category { get; }

    public string Description { get; }
}
