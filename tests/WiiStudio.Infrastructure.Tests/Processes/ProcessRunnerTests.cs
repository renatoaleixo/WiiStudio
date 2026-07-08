using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Security;

namespace WiiStudio.Infrastructure.Tests.Processes;

public sealed class ProcessRunnerTests : IDisposable
{
    private readonly string tempRoot = Path.Combine(Path.GetTempPath(), "wiistudio-process-runner-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task RunAsync_ShouldReturnFakeExecutorResultWithoutRealExternalTool()
    {
        var executable = CreateExecutable();
        var executor = new FakeProcessExecutor(new ProcessResult(0, "ok", string.Empty, TimeSpan.FromMilliseconds(5)));
        var runner = CreateRunner(executor);

        var result = await runner.RunAsync(new ProcessRequest(executable, ["--version"], TimeSpan.FromSeconds(1)), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.StandardOutput.Should().Be("ok");
        executor.Requests.Should().ContainSingle().Which.ExecutablePath.Should().Be(executable);
    }

    [Fact]
    public async Task RunAsync_ShouldPreserveFailureExitCode()
    {
        var executable = CreateExecutable();
        var runner = CreateRunner(new FakeProcessExecutor(new ProcessResult(7, string.Empty, "failed", TimeSpan.FromMilliseconds(5))));

        var result = await runner.RunAsync(new ProcessRequest(executable, [], TimeSpan.FromSeconds(1)), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ExitCode.Should().Be(7);
        result.StandardError.Should().Be("failed");
    }

    [Fact]
    public async Task RunAsync_ShouldRedactSensitiveStdoutAndStderr()
    {
        var executable = CreateExecutable();
        var runner = CreateRunner(new FakeProcessExecutor(new ProcessResult(
            1,
            "token=abc",
            "Authorization: Bearer secret",
            TimeSpan.FromMilliseconds(5))));

        var result = await runner.RunAsync(new ProcessRequest(executable, [], TimeSpan.FromSeconds(1)), CancellationToken.None);

        result.StandardOutput.Should().Be("token=[REDACTED]");
        result.StandardError.Should().Be("Authorization: Bearer [REDACTED]");
    }

    [Fact]
    public async Task RunAsync_ShouldRejectMissingExecutableBeforeExecutor()
    {
        var executor = new FakeProcessExecutor(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.Zero));
        var runner = CreateRunner(executor);

        var act = () => runner.RunAsync(new ProcessRequest(Path.Combine(tempRoot, "missing"), [], TimeSpan.FromSeconds(1)), CancellationToken.None);

        await act.Should().ThrowAsync<FileNotFoundException>();
        executor.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task RunAsync_ShouldPropagateCancellationToExecutor()
    {
        var executable = CreateExecutable();
        var runner = CreateRunner(new CancellingProcessExecutor());
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var act = () => runner.RunAsync(new ProcessRequest(executable, [], TimeSpan.FromSeconds(1)), cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    public void Dispose()
    {
        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    private ProcessRunner CreateRunner(IProcessExecutor executor) =>
        new(
            NullLogger<ProcessRunner>.Instance,
            new SafePathService(),
            new LogRedactionService(),
            executor);

    private string CreateExecutable()
    {
        Directory.CreateDirectory(tempRoot);
        var path = Path.Combine(tempRoot, "tool");
        File.WriteAllText(path, string.Empty);
        return path;
    }

    private sealed class FakeProcessExecutor : IProcessExecutor
    {
        private readonly ProcessResult result;
        private readonly List<ProcessRequest> requests = [];

        public FakeProcessExecutor(ProcessResult result)
        {
            this.result = result;
        }

        public IReadOnlyList<ProcessRequest> Requests => requests;

        public Task<ProcessResult> ExecuteAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            requests.Add(request);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(result);
        }
    }

    private sealed class CancellingProcessExecutor : IProcessExecutor
    {
        public Task<ProcessResult> ExecuteAsync(ProcessRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.Zero));
        }
    }
}
