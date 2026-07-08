namespace WiiStudio.Application.DTOs;

public enum NavigationItemGroup
{
    Principal,
    Operations,
    Management,
    System
}

public sealed record NavigationItem(
    string Id,
    string TitleKey,
    string IconKey,
    string Route,
    int Order,
    NavigationItemGroup Group,
    bool IsVisible = true,
    bool IsEnabled = true);

public sealed record NavigationState(
    string CurrentRoute,
    NavigationItem? ActiveItem,
    IReadOnlyList<string> BackStack,
    IReadOnlyList<string> ForwardStack,
    bool IsLoading,
    string? ErrorMessage);
