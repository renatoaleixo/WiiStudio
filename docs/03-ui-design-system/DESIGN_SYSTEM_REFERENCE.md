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

## F053-F060 Final Advanced UI Layer

Implemented component files:

- `src/WiiStudio.App/Styles/components/Tabs.axaml`
- `src/WiiStudio.App/Styles/components/HorizontalTabs.axaml`
- `src/WiiStudio.App/Styles/components/VerticalTabs.axaml`
- `src/WiiStudio.App/Styles/components/CompactTabs.axaml`
- `src/WiiStudio.App/Styles/components/Menus.axaml`
- `src/WiiStudio.App/Styles/components/ContextMenus.axaml`
- `src/WiiStudio.App/Styles/components/FlyoutMenus.axaml`
- `src/WiiStudio.App/Styles/components/MenuItems.axaml`
- `src/WiiStudio.App/Styles/components/Tooltips.axaml`
- `src/WiiStudio.App/Styles/components/HelpHints.axaml`
- `src/WiiStudio.App/Styles/components/InlineHelp.axaml`
- `src/WiiStudio.App/Styles/components/Badges.axaml`
- `src/WiiStudio.App/Styles/components/SeverityBadges.axaml`
- `src/WiiStudio.App/Styles/components/Chips.axaml`
- `src/WiiStudio.App/Styles/components/StatusIndicators.axaml`
- `src/WiiStudio.App/Styles/components/ProgressIndicators.axaml`
- `src/WiiStudio.App/Styles/components/Spinner.axaml`
- `src/WiiStudio.App/Styles/components/StepProgress.axaml`
- `src/WiiStudio.App/Styles/components/BatchProgress.axaml`
- `src/WiiStudio.App/Styles/components/EmptyStates.axaml`
- `src/WiiStudio.App/Styles/components/ErrorStates.axaml`
- `src/WiiStudio.App/Styles/components/LoadingStates.axaml`
- `src/WiiStudio.App/Styles/components/Toasts.axaml`
- `src/WiiStudio.App/Styles/components/Snackbars.axaml`
- `src/WiiStudio.App/Styles/components/Banners.axaml`
- `src/WiiStudio.App/Styles/components/InlineFeedback.axaml`

Implemented contract files:

- `src/WiiStudio.Application/DTOs/TabModels.cs`
- `src/WiiStudio.Application/DTOs/MenuModels.cs`
- `src/WiiStudio.Application/DTOs/FeedbackModels.cs`
- `src/WiiStudio.Application/DTOs/ProgressModels.cs`
- `src/WiiStudio.Application/DTOs/ViewStateModels.cs`
- `src/WiiStudio.Application/DTOs/ResponsiveModels.cs`
- `src/WiiStudio.Application/Interfaces/IFeedbackService.cs`
- `src/WiiStudio.Application/Interfaces/IResponsiveLayoutService.cs`

Official classes include `horizontalTabs`, `verticalTabs`, `compact`, `danger`, `flyoutMenu`, `helpHint`, `inlineHelp`, `badge`, `chip`, `statusDot`, `progressBlock`, `stepProgress`, `batchProgress`, `partialErrorState`, `loadingContent`, `toast`, `snackbar`, `banner`, and `inlineFeedback`.

The `03-ui-design-system` block is now complete through F060. New screens should reuse these resources before introducing additional local styles.
