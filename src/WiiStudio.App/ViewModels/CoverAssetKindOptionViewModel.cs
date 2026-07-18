using CommunityToolkit.Mvvm.ComponentModel;
using WiiStudio.Application.DTOs;

namespace WiiStudio.App.ViewModels;

public sealed partial class CoverAssetKindOptionViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public CoverAssetKindOptionViewModel(CoverAssetKind kind, string displayName, bool isSelected)
    {
        Kind = kind;
        DisplayName = displayName;
        IsSelected = isSelected;
    }

    public CoverAssetKind Kind { get; }

    public string DisplayName { get; }
}
