namespace Tervia.Capabilities;

/// <summary>Canonical capability identifiers. Platform agnostic on purpose.</summary>
public readonly record struct CapabilityId(string Value)
{
    public static readonly CapabilityId RemoteNavigation = new("remote.navigation");
    public static readonly CapabilityId RemoteHome = new("remote.home");
    public static readonly CapabilityId RemoteBack = new("remote.back");
    public static readonly CapabilityId RemoteKeyboard = new("remote.keyboard");

    public static readonly CapabilityId PowerReadState = new("power.read");
    public static readonly CapabilityId PowerOn = new("power.on");
    public static readonly CapabilityId PowerOff = new("power.off");

    public static readonly CapabilityId VolumeRead = new("volume.read");
    public static readonly CapabilityId VolumeChange = new("volume.change");
    public static readonly CapabilityId VolumeMute = new("volume.mute");

    public static readonly CapabilityId InputList = new("input.list");
    public static readonly CapabilityId InputSwitch = new("input.switch");

    public static readonly CapabilityId AppsList = new("apps.list");
    public static readonly CapabilityId AppsLaunch = new("apps.launch");
    public static readonly CapabilityId AppsStop = new("apps.stop");
    public static readonly CapabilityId AppsInstall = new("apps.install");
    public static readonly CapabilityId AppsRemove = new("apps.remove");

    public static readonly CapabilityId DisplayReadResolution = new("display.read.resolution");
    public static readonly CapabilityId DisplayReadPictureMode = new("display.read.picturemode");
    public static readonly CapabilityId DisplayChangePictureMode = new("display.change.picturemode");

    public static readonly CapabilityId AudioReadOutput = new("audio.read.output");
    public static readonly CapabilityId AudioChangeOutput = new("audio.change.output");

    public static readonly CapabilityId DiagnosticsSystemInfo = new("diagnostics.systeminfo");
    public static readonly CapabilityId DiagnosticsStorage = new("diagnostics.storage");
    public static readonly CapabilityId DiagnosticsNetwork = new("diagnostics.network");

    public static readonly CapabilityId DeveloperLogs = new("developer.logs");
    public static readonly CapabilityId DeveloperScreenshot = new("developer.screenshot");
    public static readonly CapabilityId DeveloperShell = new("developer.shell");

    public static readonly CapabilityId MaintenanceListPackages = new("maintenance.listpackages");
    public static readonly CapabilityId MaintenanceDisablePackage = new("maintenance.disablepackage");
    public static readonly CapabilityId MaintenanceUninstallPackage = new("maintenance.uninstallpackage");

    public static readonly CapabilityId RecoverySnapshot = new("recovery.snapshot");
    public static readonly CapabilityId RecoveryRestore = new("recovery.restore");

    public override string ToString() => Value;
}
