using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class ResponsiveLayoutServiceTests
{
    [Theory]
    [InlineData(640, Breakpoint.Compact)]
    [InlineData(1024, Breakpoint.Standard)]
    [InlineData(1440, Breakpoint.Wide)]
    [InlineData(1920, Breakpoint.UltraWide)]
    public void ResolveBreakpoint_ShouldMapOfficialWidths(double width, Breakpoint expected)
    {
        var service = new ResponsiveLayoutService();

        service.ResolveBreakpoint(width).Should().Be(expected);
    }

    [Fact]
    public void ResolveBreakpoint_ShouldRaiseEventWhenBreakpointChanges()
    {
        var service = new ResponsiveLayoutService();
        BreakpointChangedEvent? raised = null;
        service.BreakpointChanged += (_, args) => raised = args;

        service.ResolveBreakpoint(640);

        raised.Should().NotBeNull();
        raised!.Current.Should().Be(Breakpoint.Compact);
    }
}
