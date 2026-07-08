namespace WiiStudio.Application.DTOs;

public enum Breakpoint
{
    Compact,
    Standard,
    Wide,
    UltraWide
}

public sealed record WindowMetrics(
    double Width,
    double Height,
    double Scaling,
    Breakpoint Breakpoint);

public sealed record BreakpointChangedEvent(
    Breakpoint Previous,
    Breakpoint Current,
    WindowMetrics Metrics);
