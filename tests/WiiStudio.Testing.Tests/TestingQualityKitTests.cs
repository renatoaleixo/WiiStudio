using WiiStudio.Infrastructure.Processes;

namespace WiiStudio.Testing.Tests;

public sealed class TestingQualityKitTests
{
    [Fact]
    public void QualityGateEvaluator_ShouldBlockCriticalQualityFailures()
    {
        var evaluator = new QualityGateEvaluator();

        var result = evaluator.Evaluate(new QualityGateInput(
            BuildPassed: true,
            TestsPassed: false,
            HasCriticalWarnings: false,
            DocumentationUpdated: false,
            ContainsSecretLeak: true,
            UsesDirectProcessStart: false,
            HasPathTraversalRisk: false,
            SecuritySensitive: true,
            SecurityReviewed: false,
            BugFix: true,
            RegressionTestAdded: false,
            PluginFeature: false,
            PluginSecurityValidated: false,
            PerformanceSensitive: false,
            PerformanceAccepted: false));

        result.Passed.Should().BeFalse();
        result.Blockers.Should().Contain([
            "quality.gate.testsFailed",
            "quality.gate.documentationMissing",
            "quality.gate.secretLeak",
            "quality.gate.securityReviewMissing",
            "quality.gate.regressionMissing"]);
    }

    [Fact]
    public async Task FakeProcessExecutor_ShouldReturnConfiguredResult()
    {
        var executor = FakeProcessExecutor.Succeed("ok", "token=secret");
        var request = new ProcessRequest("fake-wit", ["--version"], TimeSpan.FromSeconds(1));

        var result = await executor.ExecuteAsync(request, CancellationToken.None);

        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Be("ok");
        executor.Requests.Should().ContainSingle().Which.Arguments.Should().Contain("--version");
    }

    [Fact]
    public void PerformanceBaselineRegistry_ShouldDetectBudgetRegression()
    {
        var registry = new PerformanceBaselineRegistry();
        registry.Register(new PerformanceScenario("LargeLibrarySearch", TimeSpan.FromMilliseconds(100), "Library"));

        var result = registry.Evaluate(new PerformanceMeasurement(
            "LargeLibrarySearch",
            TimeSpan.FromMilliseconds(250),
            DateTimeOffset.UtcNow));

        result.Passed.Should().BeFalse();
        result.ReasonKey.Should().Be("quality.performance.budgetExceeded");
    }

    [Fact]
    public void PluginTestManifestBuilder_ShouldCreateValidDefaultManifest()
    {
        var manifest = new PluginTestManifestBuilder()
            .WithId("com.example.quality")
            .Build();

        manifest.Id.Should().Be("com.example.quality");
        manifest.Capabilities.Should().NotBeEmpty();
        manifest.Permissions.Should().Contain(WiiStudio.Plugin.Abstractions.PluginPermission.AccessMetadata);
    }

    [Fact]
    public void TemporaryDirectoryFixture_ShouldCleanCreatedFiles()
    {
        string root;
        using (var fixture = TemporaryDirectoryFixture.Create("wiistudio-quality"))
        {
            root = fixture.RootDirectory;
            fixture.CreateFile("data/sample.txt", "hello").Should().EndWith("sample.txt");
            File.Exists(Path.Combine(root, "data", "sample.txt")).Should().BeTrue();
        }

        Directory.Exists(root).Should().BeFalse();
    }

}
