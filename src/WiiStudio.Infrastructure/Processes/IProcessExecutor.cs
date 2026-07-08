namespace WiiStudio.Infrastructure.Processes;

public interface IProcessExecutor
{
    Task<ProcessResult> ExecuteAsync(ProcessRequest request, CancellationToken cancellationToken);
}
