using Microsoft.Extensions.DependencyInjection;
using WiiStudio.Infrastructure.DependencyInjection;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Plugin.Abstractions;
using WiiStudio.Plugin.Hosting;

namespace WiiStudio.Testing;

public sealed class TemporaryDirectoryFixture : IDisposable
{
    private TemporaryDirectoryFixture(string rootDirectory)
    {
        RootDirectory = rootDirectory;
        Directory.CreateDirectory(rootDirectory);
    }

    public string RootDirectory { get; }

    public static TemporaryDirectoryFixture Create(string prefix = "wiistudio-test") =>
        new(Path.Combine(Path.GetTempPath(), $"{prefix}-{Guid.NewGuid():N}"));

    public string CreateFile(string relativePath, string content = "")
    {
        var path = Path.Combine(RootDirectory, relativePath);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(RootDirectory))
        {
            Directory.Delete(RootDirectory, recursive: true);
        }
    }
}

public sealed class IntegrationTestFixture : IAsyncDisposable
{
    private readonly TemporaryDirectoryFixture tempDirectory;

    private IntegrationTestFixture(TemporaryDirectoryFixture tempDirectory, IServiceProvider services)
    {
        this.tempDirectory = tempDirectory;
        TempRoot = tempDirectory.RootDirectory;
        Services = services;
    }

    public string TempRoot { get; }

    public IServiceProvider Services { get; }

    public static IntegrationTestFixture Create(Action<IServiceCollection>? configure = null)
    {
        var temp = TemporaryDirectoryFixture.Create("wiistudio-integration");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWiiStudioInfrastructure();
        configure?.Invoke(services);
        return new IntegrationTestFixture(temp, services.BuildServiceProvider());
    }

    public ValueTask DisposeAsync()
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        tempDirectory.Dispose();
        return ValueTask.CompletedTask;
    }
}

public sealed class DatabaseTestFixture : IDisposable
{
    private readonly TemporaryDirectoryFixture tempDirectory;

    private DatabaseTestFixture(TemporaryDirectoryFixture tempDirectory)
    {
        this.tempDirectory = tempDirectory;
        DatabasePath = Path.Combine(tempDirectory.RootDirectory, "wiistudio-test.db");
    }

    public string DatabasePath { get; }

    public static DatabaseTestFixture Create() =>
        new(TemporaryDirectoryFixture.Create("wiistudio-database"));

    public void Dispose() => tempDirectory.Dispose();
}

public sealed class FakeProcessExecutor : IProcessExecutor
{
    private readonly Queue<Func<ProcessRequest, CancellationToken, Task<ProcessResult>>> executions = new();

    public IReadOnlyList<ProcessRequest> Requests => requests;

    private readonly List<ProcessRequest> requests = [];

    public static FakeProcessExecutor Succeed(string standardOutput = "", string standardError = "", TimeSpan? duration = null) =>
        new FakeProcessExecutor().Returns(new ProcessResult(0, standardOutput, standardError, duration ?? TimeSpan.FromMilliseconds(1)));

    public FakeProcessExecutor Returns(ProcessResult result)
    {
        executions.Enqueue((_, _) => Task.FromResult(result));
        return this;
    }

    public FakeProcessExecutor Throws(Exception exception)
    {
        executions.Enqueue((_, _) => Task.FromException<ProcessResult>(exception));
        return this;
    }

    public Task<ProcessResult> ExecuteAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        requests.Add(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (executions.Count == 0)
        {
            return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty, TimeSpan.Zero));
        }

        return executions.Dequeue().Invoke(request, cancellationToken);
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RegressionCaseAttribute : Attribute
{
    public RegressionCaseAttribute(string id, string area, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(area);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Id = id;
        Area = area;
        Reason = reason;
    }

    public string Id { get; }
    public string Area { get; }
    public string Reason { get; }
}

public sealed record PerformanceScenario(string Name, TimeSpan Budget, string Area);

public sealed record PerformanceMeasurement(string ScenarioName, TimeSpan Duration, DateTimeOffset MeasuredAt);

public sealed class PerformanceBaselineRegistry
{
    private readonly Dictionary<string, PerformanceScenario> scenarios = new(StringComparer.OrdinalIgnoreCase);

    public void Register(PerformanceScenario scenario)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario.Name);
        scenarios[scenario.Name] = scenario;
    }

    public QualityCheckResult Evaluate(PerformanceMeasurement measurement)
    {
        if (!scenarios.TryGetValue(measurement.ScenarioName, out var scenario))
        {
            return QualityCheckResult.Fail("quality.performance.baselineMissing");
        }

        return measurement.Duration <= scenario.Budget
            ? QualityCheckResult.Pass()
            : QualityCheckResult.Fail("quality.performance.budgetExceeded");
    }
}

public sealed class PluginTestManifestBuilder
{
    private string id = "com.example.test";
    private string name = "Test Plugin";
    private string entryAssembly = "Test.Plugin.dll";
    private string entryType = "Test.Plugin";
    private readonly List<PluginCapability> capabilities = [PluginCapability.MetadataProvider];
    private readonly List<PluginPermission> permissions = [PluginPermission.AccessMetadata];

    public PluginTestManifestBuilder WithId(string value)
    {
        id = value;
        return this;
    }

    public PluginTestManifestBuilder WithEntry(string assembly, string type)
    {
        entryAssembly = assembly;
        entryType = type;
        return this;
    }

    public PluginTestManifestBuilder WithPermission(PluginPermission permission)
    {
        permissions.Add(permission);
        return this;
    }

    public PluginManifest Build() =>
        new(
            id,
            name,
            "Plugin created by WiiStudio.Testing.",
            "1.0.0",
            "WiiStudio Tests",
            null,
            null,
            null,
            entryAssembly,
            entryType,
            "1.0.0",
            "1.0.0",
            null,
            [],
            capabilities,
            permissions,
            [],
            new PluginResources(null, null),
            [],
            "Tests",
            null);
}

public sealed class QualityGateEvaluator
{
    public QualityGateResult Evaluate(QualityGateInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var blockers = new List<string>();
        Require(input.BuildPassed, "quality.gate.buildFailed", blockers);
        Require(input.TestsPassed, "quality.gate.testsFailed", blockers);
        Require(!input.HasCriticalWarnings, "quality.gate.criticalWarnings", blockers);
        Require(input.DocumentationUpdated, "quality.gate.documentationMissing", blockers);
        Require(!input.ContainsSecretLeak, "quality.gate.secretLeak", blockers);
        Require(!input.UsesDirectProcessStart, "quality.gate.directProcessStart", blockers);
        Require(!input.HasPathTraversalRisk, "quality.gate.pathTraversal", blockers);
        Require(input.SecurityReviewed || !input.SecuritySensitive, "quality.gate.securityReviewMissing", blockers);
        Require(input.RegressionTestAdded || !input.BugFix, "quality.gate.regressionMissing", blockers);
        Require(input.PluginSecurityValidated || !input.PluginFeature, "quality.gate.pluginSecurityMissing", blockers);
        Require(input.PerformanceAccepted || !input.PerformanceSensitive, "quality.gate.performanceMissing", blockers);

        return new QualityGateResult(blockers.Count == 0, blockers);
    }

    private static void Require(bool condition, string blocker, ICollection<string> blockers)
    {
        if (!condition)
        {
            blockers.Add(blocker);
        }
    }
}

public sealed record QualityGateInput(
    bool BuildPassed,
    bool TestsPassed,
    bool HasCriticalWarnings,
    bool DocumentationUpdated,
    bool ContainsSecretLeak,
    bool UsesDirectProcessStart,
    bool HasPathTraversalRisk,
    bool SecuritySensitive,
    bool SecurityReviewed,
    bool BugFix,
    bool RegressionTestAdded,
    bool PluginFeature,
    bool PluginSecurityValidated,
    bool PerformanceSensitive,
    bool PerformanceAccepted);

public sealed record QualityGateResult(bool Passed, IReadOnlyList<string> Blockers);

public sealed record QualityCheckResult(bool Passed, string? ReasonKey)
{
    public static QualityCheckResult Pass() => new(true, null);
    public static QualityCheckResult Fail(string reasonKey) => new(false, reasonKey);
}
