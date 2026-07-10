namespace WiiStudio.Infrastructure.Privilege;

public static class PolkitPolicyConstants
{
    public const string ActionId = "com.wiistudio.device.raw-access";
    public const string Vendor = "WiiStudio";
    public const string ExpectedPolicyPath = "/usr/share/polkit-1/actions/com.wiistudio.device.policy";
    public const string InstallerPath = "/usr/lib/wiistudio/wiistudio-polkit-installer";
    public const string InstallCommand = "install-policy";
    public const string PkexecExecutable = "pkexec";
}
