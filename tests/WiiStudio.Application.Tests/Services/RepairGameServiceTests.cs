using FluentAssertions;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;
using WiiStudio.Application.Services;

namespace WiiStudio.Application.Tests.Services;

public sealed class RepairGameServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_ShouldCreateNoActionPlanForHealthyGames()
    {
        var verifyService = new FakeVerifyGameService(new VerificationReport(
            [new VerificationResultDto("RMCP01", VerificationStatus.Healthy, "Game verified successfully.")],
            DateTimeOffset.UtcNow));
        var service = new RepairGameService(verifyService);

        var plan = await service.AnalyzeAsync(["RMCP01"], CancellationToken.None);

        var item = plan.Items.Should().ContainSingle().Subject;
        item.Action.Should().Be(RepairAction.NoAction);
        item.VerificationStatus.Should().Be(VerificationStatus.Healthy);
        plan.HasExecutableActions.Should().BeFalse();
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldPlanRevalidationForWarnings()
    {
        var verifyService = new FakeVerifyGameService(new VerificationReport(
            [new VerificationResultDto("RMCP01", VerificationStatus.Warning, "Game was not found in the library.")],
            DateTimeOffset.UtcNow));
        var service = new RepairGameService(verifyService);

        var plan = await service.AnalyzeAsync(["RMCP01"], CancellationToken.None);

        var item = plan.Items.Should().ContainSingle().Subject;
        item.Action.Should().Be(RepairAction.RevalidateWithWiimms);
        item.Risk.Should().Contain("Low");
        plan.HasExecutableActions.Should().BeTrue();
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldRequireReimportForCorruptedGames()
    {
        var verifyService = new FakeVerifyGameService(new VerificationReport(
            [new VerificationResultDto("RMCP01", VerificationStatus.Corrupted, "Game verification failed.")],
            DateTimeOffset.UtcNow));
        var service = new RepairGameService(verifyService);

        var plan = await service.AnalyzeAsync(["RMCP01"], CancellationToken.None);

        plan.Items.Should().ContainSingle().Which.Action.Should().Be(RepairAction.ReimportRequired);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRevalidateWarningsSuccessfully()
    {
        var verifyService = new SequenceVerifyGameService(
            new VerificationReport([new VerificationResultDto("RMCP01", VerificationStatus.Healthy, "Game verified successfully.")], DateTimeOffset.UtcNow));
        var service = new RepairGameService(verifyService);
        var plan = new RepairPlan(
            [new RepairPlanItem("RMCP01", RepairAction.RevalidateWithWiimms, VerificationStatus.Warning, "Warning.", "Low.")],
            DateTimeOffset.UtcNow);

        var result = await service.ExecuteAsync(plan, CancellationToken.None);

        result.HasFailures.Should().BeFalse();
        result.Items.Should().ContainSingle().Which.Status.Should().Be(RepairStatus.Completed);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnRequiresUserActionForReimportRequired()
    {
        var service = new RepairGameService(new FakeVerifyGameService(new VerificationReport([], DateTimeOffset.UtcNow)));
        var plan = new RepairPlan(
            [new RepairPlanItem("RMCP01", RepairAction.ReimportRequired, VerificationStatus.Corrupted, "Corrupted.", "High.")],
            DateTimeOffset.UtcNow);

        var result = await service.ExecuteAsync(plan, CancellationToken.None);

        var item = result.Items.Should().ContainSingle().Subject;
        item.Status.Should().Be(RepairStatus.RequiresUserAction);
        item.Message.Should().Be("The game must be reimported.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSkipNoActionItems()
    {
        var service = new RepairGameService(new FakeVerifyGameService(new VerificationReport([], DateTimeOffset.UtcNow)));
        var plan = new RepairPlan(
            [new RepairPlanItem("RMCP01", RepairAction.NoAction, VerificationStatus.Healthy, "Healthy.", "None.")],
            DateTimeOffset.UtcNow);

        var result = await service.ExecuteAsync(plan, CancellationToken.None);

        result.Items.Should().ContainSingle().Which.Status.Should().Be(RepairStatus.Skipped);
    }

    private sealed class FakeVerifyGameService : IVerifyGameService
    {
        private readonly VerificationReport _report;

        public FakeVerifyGameService(VerificationReport report)
        {
            _report = report;
        }

        public Task<VerificationReport> VerifyAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_report);
        }
    }

    private sealed class SequenceVerifyGameService : IVerifyGameService
    {
        private readonly Queue<VerificationReport> _reports;

        public SequenceVerifyGameService(params VerificationReport[] reports)
        {
            _reports = new Queue<VerificationReport>(reports);
        }

        public Task<VerificationReport> VerifyAsync(IReadOnlyList<string> gameIds, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_reports.Dequeue());
        }
    }
}
