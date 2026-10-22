using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed record CompatibilityOptionViewModel(LocalCompatibilityStatus Status, string Name);
