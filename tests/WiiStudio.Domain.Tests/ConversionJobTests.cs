using FluentAssertions;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;

namespace WiiStudio.Domain.Tests;

public sealed class ConversionJobTests
{
    [Fact]
    public void Start_ShouldMovePendingJobToRunning()
    {
        var job = CreateJob();

        job.Start();

        job.Status.Should().Be(ConversionJobStatus.Running);
    }

    [Fact]
    public void Complete_ShouldRequireRunningJob()
    {
        var job = CreateJob();

        var act = job.Complete;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Fail_ShouldRecordErrorMessageAndFinishJob()
    {
        var job = CreateJob();
        job.Start();

        job.Fail("Conversion failed");

        job.Status.Should().Be(ConversionJobStatus.Failed);
        job.ErrorMessage.Should().Be("Conversion failed");
        job.FinishedAt.Should().NotBeNull();
    }

    private static ConversionJob CreateJob()
    {
        return new ConversionJob(
            Guid.NewGuid(),
            "/games/source.iso",
            "/games/output.wbfs",
            FileFormat.Iso,
            FileFormat.Wbfs);
    }
}
