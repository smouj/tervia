using Tervia.Capabilities;
using Tervia.Core;
using Tervia.Safety;
using Xunit;

namespace Tervia.Safety.Tests;

public sealed class SafetyTests
{
    private static readonly TvSafetyEngine Engine = new(
    [
        new AndroidTvSafetyPolicy(),
        new TizenSafetyPolicy(),
        new WebOsSafetyPolicy(),
        new VidaaSafetyPolicy(),
    ]);

    [Fact]
    public void Unknown_risk_requires_confirmation()
    {
        var decision = Engine.Evaluate(new SafetyRequest
        {
            OperationId = "something.new",
            Risk = RiskLevel.Unknown,
            TvId = "tv",
            Platform = TvPlatform.RokuTV,
        });

        Assert.Equal(SafetyOutcome.RequiresConfirmation, decision.Outcome);
    }

    [Fact]
    public void Critical_risk_is_denied()
    {
        var decision = Engine.Evaluate(new SafetyRequest
        {
            OperationId = "firmware.flash",
            Risk = RiskLevel.Critical,
            TvId = "tv",
            Platform = TvPlatform.AndroidTV,
        });

        Assert.Equal(SafetyOutcome.Denied, decision.Outcome);
    }

    [Theory]
    [InlineData("package.disable.launcher")]
    [InlineData("package.disable.packageinstaller")]
    [InlineData("package.disable.network")]
    public void Android_critical_packages_are_denied(string operation)
    {
        var decision = Engine.Evaluate(new SafetyRequest
        {
            OperationId = operation,
            Risk = RiskLevel.Medium,
            TvId = "tv",
            Platform = TvPlatform.AndroidTV,
        });

        Assert.Equal(SafetyOutcome.Denied, decision.Outcome);
    }

    [Fact]
    public void Medium_risk_requires_confirmation()
    {
        var decision = Engine.Evaluate(new SafetyRequest
        {
            OperationId = "package.uninstall",
            Risk = RiskLevel.Medium,
            TvId = "tv",
            Platform = TvPlatform.AndroidTV,
        });

        Assert.Equal(SafetyOutcome.RequiresConfirmation, decision.Outcome);
    }

    [Fact]
    public void Low_risk_is_allowed()
    {
        var decision = Engine.Evaluate(new SafetyRequest
        {
            OperationId = "remote.press",
            Risk = RiskLevel.Low,
            TvId = "tv",
            Platform = TvPlatform.RokuTV,
        });

        Assert.Equal(SafetyOutcome.Allowed, decision.Outcome);
    }

    [Fact]
    public void Tizen_signing_bypass_is_denied()
    {
        var decision = Engine.Evaluate(new SafetyRequest
        {
            OperationId = "signing.disable-check",
            Risk = RiskLevel.Low,
            TvId = "tv",
            Platform = TvPlatform.Tizen,
        });

        Assert.Equal(SafetyOutcome.Denied, decision.Outcome);
    }
}
