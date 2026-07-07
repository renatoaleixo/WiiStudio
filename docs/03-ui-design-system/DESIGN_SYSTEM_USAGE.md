# WiiStudio Design System Usage

This document defines how UI work should consume the WiiStudio design system.

## Resource Location

- Global entry point: `src/WiiStudio.App/Styles/DesignSystem.axaml`
- Visual tokens: `src/WiiStudio.App/Styles/tokens/`
- Component styles: `src/WiiStudio.App/Styles/components/`
- Theme dictionaries: `src/WiiStudio.App/Styles/themes/`

## Rules

- Use semantic resources such as `WiiBrushSurface`, `WiiBrushTextPrimary`, and `WiiBrushBorder`.
- Do not add hardcoded colors in views or components.
- Add new colors only in `Colors.axaml`, with both light and dark values.
- Use spacing tokens for reusable component defaults.
- Keep component styles in the `components` folder.
- Every interactive component must have visible default, hover, pressed, disabled, and focus states when applicable.
- New screens must work with `ThemeVariant.Light`, `ThemeVariant.Dark`, and system theme.

## Current Foundation

F041 introduces the shared resource dictionaries and migrates the main shell colors to semantic tokens. Later design system features refine the token set and component-specific details.

F042-F046 extend this foundation with official color palettes, typography resources, icon registrations, spacing/layout tokens, and button variants. See `DESIGN_SYSTEM_REFERENCE.md` for the implemented resource names.
