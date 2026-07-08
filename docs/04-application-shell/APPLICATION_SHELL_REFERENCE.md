# WiiStudio Application Shell Reference

This reference documents the implemented F061-F070 application shell foundation.

## Visual Shell

Implemented in `src/WiiStudio.App/Views/MainWindow.axaml`:

- Header and global toolbar region.
- Main Menu with official groups: Arquivo, Editar, Exibir, Biblioteca, Ferramentas, Plugins, Ajuda.
- Sidebar navigation region.
- Workspace region.
- Right panel region.
- Footer/status area.
- Notification host placeholder.
- Dialog host/scrim placeholder.
- Overlay/loading host placeholder.

The view reuses the completed `03-ui-design-system` styles for toolbar, navigation, data grid, badges, feedback, loading and modal surfaces.

## Shell Contracts

Implemented DTOs:

- `AppShellModels.cs`
- `WindowManagementModels.cs`
- `KeyboardShortcutModels.cs`
- `AccessibilityModels.cs`

Implemented interfaces:

- `IRouteViewRegistry`
- `IPanelService`
- `IWindowStateService`
- `ICloseGuardService`
- `IKeyboardShortcutRegistry`
- `IKeyboardShortcutService`

Existing shell-related contracts also remain active:

- `IMainWindowService`
- `IDockService`
- `IStatusBarService`
- `INavigationService`
- `IResponsiveLayoutService`

## Services

Implemented services:

- `KeyboardShortcutRegistry`
- `KeyboardShortcutService`
- `ResponsiveLayoutService`
- `RouteViewRegistry`
- `WindowStateService`
- `CloseGuardService`
- `PanelService`

These services are registered in dependency injection through `AddWiiStudioApplication`.

## Validation

Application shell tests cover:

- Shortcut registration and duplicate detection.
- Scope-specific shortcut lookup.
- Official responsive breakpoint resolution.
- Breakpoint change notification.

Future feature screens should use these shell contracts instead of adding route, shortcut, panel, window or close behavior directly in views or code-behind.
