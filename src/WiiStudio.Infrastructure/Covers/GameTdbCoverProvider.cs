using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Covers;

public sealed class GameTdbCoverProvider : ICoverProvider
{
    private readonly HttpClient _httpClient;
    private readonly IGameTdbCoverUrlBuilder _urlBuilder;
    private readonly IImageValidator _imageValidator;
    private readonly GameTdbCoverProviderOptions _options;

    public GameTdbCoverProvider(
        HttpClient httpClient,
        IGameTdbCoverUrlBuilder urlBuilder,
        IImageValidator imageValidator,
        GameTdbCoverProviderOptions options)
    {
        _httpClient = httpClient;
        _urlBuilder = urlBuilder;
        _imageValidator = imageValidator;
        _options = options;
    }

    public async Task<Stream?> DownloadAsync(string gameId, CoverAssetKind kind, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.AllowAutomaticCoverDownload)
        {
            return null;
        }

        IReadOnlyList<Uri> urls;
        try
        {
            urls = _urlBuilder.BuildCandidateUrls(gameId, kind);
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }

        if (urls.Count == 0)
        {
            return null;
        }

        foreach (var url in urls)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var content = await TryDownloadAsync(url, kind, cancellationToken);
            if (content is not null)
            {
                return content;
            }
        }

        return null;
    }

    private async Task<Stream?> TryDownloadAsync(Uri url, CoverAssetKind kind, CancellationToken cancellationToken)
    {
        if (url.Scheme != Uri.UriSchemeHttps)
        {
            return null;
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.Timeout);

        try
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrWhiteSpace(contentType) ||
                !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(timeout.Token);
            var bytes = await ReadLimitedAsync(responseStream, GetMaxBytes(kind), timeout.Token);
            if (bytes is null || !_imageValidator.IsValidImage(bytes, contentType))
            {
                return null;
            }

            return new MemoryStream(bytes, writable: false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    private long GetMaxBytes(CoverAssetKind kind)
    {
        return kind == CoverAssetKind.BackCover ? _options.MaxFullCoverBytes : _options.MaxImageBytes;
    }

    private static async Task<byte[]?> ReadLimitedAsync(Stream stream, long maxBytes, CancellationToken cancellationToken)
    {
        var memory = new MemoryStream();
        var buffer = new byte[81920];
        long totalBytes = 0;

        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                return memory.ToArray();
            }

            totalBytes += read;
            if (totalBytes > maxBytes)
            {
                return null;
            }

            memory.Write(buffer, 0, read);
        }
    }
}
