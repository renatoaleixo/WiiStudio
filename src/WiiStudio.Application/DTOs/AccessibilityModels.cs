namespace WiiStudio.Application.DTOs;

public enum AccessibilityLandmark
{
    Header,
    Navigation,
    Main,
    Complementary,
    Status,
    Dialog,
    Menu
}

public sealed record AccessibilityNavigationRule(
    AccessibilityLandmark Landmark,
    int TabOrder,
    string NameKey,
    bool RequiresKeyboardAccess = true);
