using System.Xml.Linq;
using WiiStudio.Application.DTOs;
using WiiStudio.Application.Interfaces;

namespace WiiStudio.Infrastructure.Privilege;

public sealed class PolkitPolicyValidator : IPolkitPolicyValidator
{
    public Task<PolkitPolicyValidationResult> ValidateSourcePolicyAsync(
        string sourcePolicyPath,
        CancellationToken cancellationToken = default)
    {
        return ValidateAsync(sourcePolicyPath, requireExpectedPath: false, cancellationToken);
    }

    public Task<PolkitPolicyValidationResult> ValidateInstalledPolicyAsync(
        string installedPolicyPath,
        CancellationToken cancellationToken = default)
    {
        return ValidateAsync(installedPolicyPath, requireExpectedPath: true, cancellationToken);
    }

    private static Task<PolkitPolicyValidationResult> ValidateAsync(
        string policyPath,
        bool requireExpectedPath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyPath);
        cancellationToken.ThrowIfCancellationRequested();

        var errors = new List<string>();
        var fullPath = Path.GetFullPath(policyPath);
        if (requireExpectedPath && !string.Equals(fullPath, PolkitPolicyConstants.ExpectedPolicyPath, StringComparison.Ordinal))
        {
            errors.Add("Policy path is not the expected system path.");
        }

        if (!File.Exists(fullPath))
        {
            errors.Add("Policy file was not found.");
            return Task.FromResult(CreateResult(fullPath, errors, hasAction: false, hasAuthAdminKeep: false, hasVendor: false));
        }

        try
        {
            var document = XDocument.Load(fullPath, LoadOptions.None);
            var root = document.Root;
            if (root?.Name.LocalName != "policyconfig")
            {
                errors.Add("Policy root element is not policyconfig.");
            }

            var vendor = root?.Elements().FirstOrDefault(element => element.Name.LocalName == "vendor")?.Value.Trim();
            var hasVendor = string.Equals(vendor, PolkitPolicyConstants.Vendor, StringComparison.Ordinal);
            if (!hasVendor)
            {
                errors.Add("Policy vendor is not WiiStudio.");
            }

            var actions = root?.Elements().Where(element => element.Name.LocalName == "action").ToArray() ?? [];
            var expectedAction = actions.FirstOrDefault(element =>
                string.Equals((string?)element.Attribute("id"), PolkitPolicyConstants.ActionId, StringComparison.Ordinal));
            var hasAction = expectedAction is not null;
            if (!hasAction)
            {
                errors.Add("Policy action com.wiistudio.device.raw-access was not found.");
            }

            if (actions.Any(action => !string.Equals((string?)action.Attribute("id"), PolkitPolicyConstants.ActionId, StringComparison.Ordinal)))
            {
                errors.Add("Policy contains unexpected action ids.");
            }

            var hasAuthAdminKeep = expectedAction?
                .Descendants()
                .Any(element => element.Name.LocalName is "allow_active" && element.Value.Trim() == "auth_admin_keep") == true;
            if (!hasAuthAdminKeep)
            {
                errors.Add("Policy does not contain allow_active auth_admin_keep.");
            }

            return Task.FromResult(CreateResult(fullPath, errors, hasAction, hasAuthAdminKeep, hasVendor));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Xml.XmlException)
        {
            errors.Add(exception.Message);
            return Task.FromResult(CreateResult(fullPath, errors, hasAction: false, hasAuthAdminKeep: false, hasVendor: false));
        }
    }

    private static PolkitPolicyValidationResult CreateResult(
        string policyPath,
        IReadOnlyList<string> errors,
        bool hasAction,
        bool hasAuthAdminKeep,
        bool hasVendor)
    {
        return new PolkitPolicyValidationResult(
            errors.Count == 0,
            errors.Count == 0 ? PolkitPolicyInstallState.Installed : PolkitPolicyInstallState.Invalid,
            policyPath,
            hasAction,
            hasAuthAdminKeep,
            hasVendor,
            errors);
    }
}
