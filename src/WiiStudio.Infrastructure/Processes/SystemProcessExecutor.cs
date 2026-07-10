using System.Diagnostics;

namespace WiiStudio.Infrastructure.Processes;

public sealed class SystemProcessExecutor : IProcessExecutor
{
    public async Task<ProcessResult> ExecuteAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        using var timeoutSource = new CancellationTokenSource(request.Timeout);
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

        var stopwatch = Stopwatch.StartNew();
        using var process = CreateProcess(request);

        process.Start();

        var standardOutputTask = ReadToEndAsync(process.StandardOutput, request.OnStandardOutputLine, linkedSource.Token);
        var standardErrorTask = ReadToEndAsync(process.StandardError, request.OnStandardErrorLine, linkedSource.Token);

        try
        {
            await process.WaitForExitAsync(linkedSource.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            KillProcess(process);
            throw new TimeoutException($"Process '{request.ExecutablePath}' exceeded timeout {request.Timeout}.");
        }

        stopwatch.Stop();

        return new ProcessResult(
            process.ExitCode,
            await standardOutputTask,
            await standardErrorTask,
            stopwatch.Elapsed);
    }

    private static async Task<string> ReadToEndAsync(
        StreamReader reader,
        Action<string>? onLine,
        CancellationToken cancellationToken)
    {
        if (onLine is null)
        {
            return await reader.ReadToEndAsync(cancellationToken);
        }

        var builder = new System.Text.StringBuilder();
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            builder.AppendLine(line);
            onLine(line);
        }

        return builder.ToString();
    }

    private static Process CreateProcess(ProcessRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        foreach (var argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return new Process
        {
            StartInfo = startInfo
        };
    }

    private static void KillProcess(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }
    }
}
