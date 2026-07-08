using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class KeyboardShortcutRegistryTests
{
    [Fact]
    public void TryGetByGesture_ShouldResolveScopeSpecificShortcut()
    {
        var registry = new KeyboardShortcutRegistry();
        var shortcut = new KeyboardShortcut("refresh", "shell.refresh", "Ctrl+R", "Shortcut.Refresh", ShortcutScope.Shell);

        registry.Register(shortcut);

        registry.TryGetByGesture("ctrl+r", ShortcutScope.Shell, out var resolved).Should().BeTrue();
        resolved.Should().Be(shortcut);
    }

    [Fact]
    public void Register_ShouldRejectDuplicateGestureInSameScope()
    {
        var registry = new KeyboardShortcutRegistry();
        registry.Register(new KeyboardShortcut("one", "command.one", "Ctrl+R", "Shortcut.One", ShortcutScope.Global));

        var act = () => registry.Register(new KeyboardShortcut("two", "command.two", "Ctrl+R", "Shortcut.Two", ShortcutScope.Global));

        act.Should().Throw<InvalidOperationException>();
    }
}
