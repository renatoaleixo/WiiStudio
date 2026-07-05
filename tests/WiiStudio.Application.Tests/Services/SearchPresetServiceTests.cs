using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Services;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests.Services;

public sealed class SearchPresetServiceTests
{
    [Fact]
    public async Task SaveAsync_ShouldTrimNameAndStorePreset()
    {
        var service = new SearchPresetService();
        var request = new SearchRequest(SearchText: "mario", Formats: new HashSet<FileFormat> { FileFormat.Wbfs });

        var preset = await service.SaveAsync("  Favorites  ", request, CancellationToken.None);
        var presets = await service.ListAsync(CancellationToken.None);

        preset.Name.Should().Be("Favorites");
        preset.Request.Should().Be(request);
        presets.Should().ContainSingle().Which.Should().Be(preset);
    }

    [Fact]
    public async Task ListAsync_ShouldSortPresetsByName()
    {
        var service = new SearchPresetService();

        await service.SaveAsync("Zelda", new SearchRequest(SearchText: "zelda"), CancellationToken.None);
        await service.SaveAsync("Mario", new SearchRequest(SearchText: "mario"), CancellationToken.None);

        var presets = await service.ListAsync(CancellationToken.None);

        presets.Select(preset => preset.Name).Should().Equal("Mario", "Zelda");
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeletePreset()
    {
        var service = new SearchPresetService();
        var preset = await service.SaveAsync("Mario", new SearchRequest(SearchText: "mario"), CancellationToken.None);

        await service.RemoveAsync(preset.Id, CancellationToken.None);
        var presets = await service.ListAsync(CancellationToken.None);

        presets.Should().BeEmpty();
    }
}
