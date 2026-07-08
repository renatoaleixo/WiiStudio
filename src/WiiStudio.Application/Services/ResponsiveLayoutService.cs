using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Application.Services;

public sealed class ResponsiveLayoutService : IResponsiveLayoutService
{
    public Breakpoint CurrentBreakpoint { get; private set; } = Breakpoint.Standard;

    public bool IsCompact => CurrentBreakpoint == Breakpoint.Compact;

    public bool IsWide => CurrentBreakpoint is Breakpoint.Wide or Breakpoint.UltraWide;

    public event EventHandler<BreakpointChangedEvent>? BreakpointChanged;

    public Breakpoint ResolveBreakpoint(double width)
    {
        var next = width switch
        {
            <= 800 => Breakpoint.Compact,
            <= 1200 => Breakpoint.Standard,
            <= 1600 => Breakpoint.Wide,
            _ => Breakpoint.UltraWide
        };

        if (next != CurrentBreakpoint)
        {
            var previous = CurrentBreakpoint;
            CurrentBreakpoint = next;
            BreakpointChanged?.Invoke(this, new BreakpointChangedEvent(previous, next, new WindowMetrics(width, 0, 1, next)));
        }

        return next;
    }
}
