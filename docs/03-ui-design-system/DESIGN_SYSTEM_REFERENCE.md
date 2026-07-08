# WiiStudio Design System Reference

This reference documents the implemented F042-F052 design system layer.

## F042 Color System

Implemented files:

- `src/WiiStudio.App/Styles/tokens/Colors.axaml`
- `src/WiiStudio.App/Styles/tokens/LightColors.axaml`
- `src/WiiStudio.App/Styles/tokens/DarkColors.axaml`

Use semantic brushes in views and components:

- `WiiBrushPrimary`
- `WiiBrushSecondary`
- `WiiBrushAccent`
- `WiiBrushBackground`
- `WiiBrushChrome`
- `WiiBrushSidebar`
- `WiiBrushSurface`
- `WiiBrushSurfaceAlt`
- `WiiBrushSurfaceRaised`
- `WiiBrushBorder`
- `WiiBrushTextPrimary`
- `WiiBrushTextSecondary`
- `WiiBrushTextMuted`
- `WiiBrushTextInverted`
- `WiiBrushSuccess`
- `WiiBrushWarning`
- `WiiBrushError`
- `WiiBrushInfo`
- `WiiBrushDisabled`
- `WiiBrushFocus`

Hardcoded colors are allowed only inside token dictionaries.

## F043 Typography

Implemented files:

- `src/WiiStudio.App/Styles/tokens/Typography.axaml`
- `src/WiiStudio.App/Styles/tokens/FontSizes.axaml`
- `src/WiiStudio.App/Styles/tokens/FontWeights.axaml`
- `src/WiiStudio.App/Styles/components/TextStyles.axaml`

Official text classes:

- `display`
- `heading`
- `title`
- `subtitle`
- `caption`
- `micro`
- `code`

Views should use typography tokens or official classes instead of arbitrary `FontSize` values.

## F044 Iconography

Implemented files:

- `src/WiiStudio.App/Styles/icons/FluentIcons.axaml`
- `src/WiiStudio.App/Styles/icons/IconTokens.axaml`
- `src/WiiStudio.App/Styles/icons/IconStyles.axaml`

Registered semantic icons include:

- `WiiIcon.Search`
- `WiiIcon.Refresh`
- `WiiIcon.Save`
- `WiiIcon.Folder`
- `WiiIcon.Import`
- `WiiIcon.Convert`
- `WiiIcon.Settings`
- `WiiIcon.Warning`
- `WiiIcon.Error`
- `WiiIcon.Success`
- `WiiIcon.Device`
- `WiiIcon.Library`
- `WiiIcon.Favorite`
- `WiiIcon.Close`

Use `PathIcon` with registered `StreamGeometry` resources. Icon-only buttons must define `ToolTip.Tip`.

## F045 Spacing & Layout

Implemented files:

- `src/WiiStudio.App/Styles/tokens/Spacing.axaml`
- `src/WiiStudio.App/Styles/tokens/Layout.axaml`
- `src/WiiStudio.App/Styles/tokens/Grid.axaml`
- `src/WiiStudio.App/Styles/tokens/Breakpoints.axaml`
- `src/WiiStudio.App/Styles/tokens/Responsive.axaml`

Spacing follows the 4 px scale with an additional `WiiSpace2` for rare fine adjustments. Layout tokens define page, panel, dialog, card, toolbar, sidebar, status bar, dialog, and breakpoint dimensions.

## F046 Buttons

Implemented files:

- `src/WiiStudio.App/Styles/components/ButtonTokens.axaml`
- `src/WiiStudio.App/Styles/components/Buttons.axaml`
- `src/WiiStudio.App/Styles/components/IconButtons.axaml`

Official button classes:

- `primary`
- `secondary`
- `tertiary`
- `danger`
- `success`
- `small`
- `large`
- `extraLarge`
- `icon`
- `loading`

Buttons should bind to commands and avoid inline colors, padding, or business logic.

## F047 Inputs & Forms

Implemented files:

- `src/WiiStudio.App/Styles/components/FormTokens.axaml`
- `src/WiiStudio.App/Styles/components/Inputs.axaml`
- `src/WiiStudio.App/Styles/components/Forms.axaml`
- `src/WiiStudio.App/Styles/components/Validation.axaml`
- `src/WiiStudio.App/Styles/components/SearchBox.axaml`
- `src/WiiStudio.App/Styles/components/FilePicker.axaml`
- `src/WiiStudio.App/Styles/components/DirectoryPicker.axaml`

Official classes include `form`, `formSection`, `field`, `fieldLabel`, `fieldHelp`, `validationError`, `validationSuccess`, and `searchBox`.

## F048 Dialogs & Modals

Implemented files:

- `src/WiiStudio.App/Styles/components/DialogTokens.axaml`
- `src/WiiStudio.App/Styles/components/Dialogs.axaml`
- `src/WiiStudio.App/Styles/components/Modals.axaml`
- `src/WiiStudio.Application/DTOs/DialogModels.cs`
- `src/WiiStudio.Application/Interfaces/IDialogService.cs`

Official dialog classes include `dialog`, `small`, `medium`, `large`, `extraLarge`, `dialogContent`, `dialogActions`, `modalScrim`, and `flyoutPanel`.

## F049 DataGrid

Implemented files:

- `src/WiiStudio.App/Styles/components/DataGridTokens.axaml`
- `src/WiiStudio.App/Styles/components/DataGrid.axaml`
- `src/WiiStudio.App/Styles/components/DataGridToolbar.axaml`
- `src/WiiStudio.App/Styles/components/DataGridStates.axaml`
- `src/WiiStudio.Application/DTOs/DataGridModels.cs`

Official classes include `dataGrid`, `dataGridHeader`, `dataGridHeaderText`, `dataGridRow`, `dataGridToolbar`, and `gridState`.

## F050 Cards & Containers

Implemented files:

- `src/WiiStudio.App/Styles/components/CardTokens.axaml`
- `src/WiiStudio.App/Styles/components/Cards.axaml`
- `src/WiiStudio.App/Styles/components/Containers.axaml`
- `src/WiiStudio.App/Styles/components/Panels.axaml`
- `src/WiiStudio.App/Styles/components/Sections.axaml`
- `src/WiiStudio.App/Styles/components/Tiles.axaml`
- `src/WiiStudio.App/Styles/components/StateContainers.axaml`
- `src/WiiStudio.Application/DTOs/CardModels.cs`

Official classes include `card`, `panel`, `overlay`, `tile`, `metricTile`, `container`, `contentBlock`, `sidePanel`, `section`, `emptyState`, `errorState`, and `loadingState`.

## F051 Navigation System

Implemented files:

- `src/WiiStudio.App/Styles/components/NavigationTokens.axaml`
- `src/WiiStudio.App/Styles/components/Navigation.axaml`
- `src/WiiStudio.Application/DTOs/NavigationModels.cs`
- `src/WiiStudio.Application/Interfaces/INavigationService.cs`

Official classes include `navigation`, `navigationItem`, `active`, and `navigationGroup`.

## F052 Toolbar System

Implemented files:

- `src/WiiStudio.App/Styles/components/ToolbarTokens.axaml`
- `src/WiiStudio.App/Styles/components/Toolbars.axaml`
- `src/WiiStudio.App/Styles/components/ToolbarButtons.axaml`
- `src/WiiStudio.App/Styles/components/ToolbarOverflow.axaml`
- `src/WiiStudio.Application/DTOs/ToolbarModels.cs`

Official classes include `toolbar`, `toolbarGroup`, `toolbarSeparator`, `toolbarButton`, `toolbarPrimary`, `toolbarDanger`, and `toolbarOverflow`.
