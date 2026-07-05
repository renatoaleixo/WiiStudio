using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Queries;
using WiiStudio.Application.UseCases;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Application.Tests;

public sealed class ListGamesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldMapQueryToSearchRequest()
    {
        var expectedResult = new SearchResult([], 0, DateTimeOffset.UtcNow);
        var searchService = new FakeGameSearchService(expectedResult);
        var useCase = new ListGamesUseCase(searchService);
        var driveId = Guid.NewGuid();
        var query = new GetGamesQuery(
            driveId,
            "mario",
            new HashSet<Region> { Region.Pal },
            new HashSet<FileFormat> { FileFormat.Wbfs },
            MinimumSizeInBytes: 100,
            MaximumSizeInBytes: 2000,
            SortField: GameSortField.Size,
            SortDescending: true);

        var result = await useCase.ExecuteAsync(query, CancellationToken.None);

        result.Should().BeSameAs(expectedResult);
        searchService.LastRequest.Should().NotBeNull();
        searchService.LastRequest!.UsbDriveId.Should().Be(driveId);
        searchService.LastRequest.SearchText.Should().Be("mario");
        searchService.LastRequest.Regions.Should().BeEquivalentTo([Region.Pal]);
        searchService.LastRequest.Formats.Should().BeEquivalentTo([FileFormat.Wbfs]);
        searchService.LastRequest.MinimumSizeInBytes.Should().Be(100);
        searchService.LastRequest.MaximumSizeInBytes.Should().Be(2000);
        searchService.LastRequest.SortField.Should().Be(GameSortField.Size);
        searchService.LastRequest.SortDescending.Should().BeTrue();
    }

    private sealed class FakeGameSearchService : IGameSearchService
    {
        private readonly SearchResult _result;

        public FakeGameSearchService(SearchResult result)
        {
            _result = result;
        }

        public SearchRequest? LastRequest { get; private set; }

        public Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRequest = request;

            return Task.FromResult(_result);
        }
    }
}
