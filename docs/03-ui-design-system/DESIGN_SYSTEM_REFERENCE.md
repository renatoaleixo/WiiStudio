# WiiStudio Design System Reference

This reference documents the implemented F042-F046 design system layer.

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
