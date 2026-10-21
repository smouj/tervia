using Tervia.Core;

namespace Tervia.Safety;

public sealed class AndroidTvSafetyPolicy : DefaultPolicy
{
    public override TvPlatform Platform => TvPlatform.AndroidTV;

    public override SafetyDecision Evaluate(SafetyRequest request)
    {
        var id = request.OperationId;

        if (id is "firmware.flash" or "root" or "bootloader.unlock")
        {
            return SafetyDecision.Deny("Tervia does not modify firmware or bypass device security.");
        }

        if (id is "package.disable.launcher" or "package.disable.packageinstaller" or "package.disable.network")
        {
            return SafetyDecision.Deny("Disabling a critical system package can brick the television.");
        }

        if (id is "package.disable" or "package.uninstall" or "package.clear")
        {
            return SafetyDecision.Confirm("Package changes are reversible only when the package is reinstalled.");
        }

        return base.Evaluate(request);
    }
}

public sealed class TizenSafetyPolicy : DefaultPolicy
{
    public override TvPlatform Platform => TvPlatform.Tizen;

    public override SafetyDecision Evaluate(SafetyRequest request)
    {
        if (request.OperationId.StartsWith("signing.", StringComparison.Ordinal) ||
            request.OperationId.StartsWith("certificate.", StringComparison.Ordinal))
        {
            return SafetyDecision.Deny("Tervia never bypasses Samsung signing requirements.");
        }

        return base.Evaluate(request);
    }
}

public sealed class WebOsSafetyPolicy : DefaultPolicy
{
    public override TvPlatform Platform => TvPlatform.WebOS;

    public override SafetyDecision Evaluate(SafetyRequest request)
    {
        if (request.OperationId.StartsWith("developer.keys.", StringComparison.Ordinal))
        {
            return SafetyDecision.Deny("Tervia does not generate or bypass LG developer keys.");
        }

        return base.Evaluate(request);
    }
}

public sealed class RokuSafetyPolicy : DefaultPolicy
{
    public override TvPlatform Platform => TvPlatform.RokuTV;
}

public sealed class VidaaSafetyPolicy : DefaultPolicy
{
    public override TvPlatform Platform => TvPlatform.Vidaa;

    public override SafetyDecision Evaluate(SafetyRequest request)
    {
        if (request.Risk == Capabilities.RiskLevel.Unknown)
        {
            return SafetyDecision.Deny("VIDAA local control is not verified for this operation.");
        }

        return base.Evaluate(request);
    }
}
