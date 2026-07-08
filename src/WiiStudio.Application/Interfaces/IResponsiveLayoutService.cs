using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IResponsiveLayoutService
{
    Breakpoint CurrentBreakpoint { get; }

    bool IsCompact { get; }

    bool IsWide { get; }

    event EventHandler<BreakpointChangedEvent>? BreakpointChanged;

    Breakpoint ResolveBreakpoint(double width);
}
