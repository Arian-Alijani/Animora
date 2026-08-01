using System.Text.RegularExpressions;
using FluentValidation;

namespace Animora.SharedKernel.Validation.Identity;

/// <summary>
/// I/O-free structural and format rules for <see cref="IRoleInput"/> (SH-05, CONV-18).
/// </summary>
/// <remarks>
/// This validator judges the *shape* of the claim keys, never their membership in the catalog: the
/// catalog is the Identity module's (SEC-09), and reaching it from here would either duplicate it
/// into this assembly or turn a shared rule into a lookup (SH-05). The handler rejects an unknown
/// key with an <c>ERR-IDENTITY-{NNN}</c> code, and the same handler enforces SEC-11's owner-admin
/// guard, which is a rule about one specific role rather than about role input in general.
/// </remarks>
public sealed class RoleValidator : AbstractValidator<IRoleInput>
{
    /// <summary>Longest accepted <see cref="IRoleInput.DisplayName"/>.</summary>
    public const int DisplayNameMaximumLength = 100;

    // {resource}.{action} in lower-case kebab segments (AG-12), e.g. "cash-session.open" or
    // "invoices.void". Two segments is the catalog's shape today; the pattern accepts more so a
    // future "reports.export.advanced" needs no rule change.
    private static readonly Regex ClaimKeyPattern = new(
        "^[a-z][a-z0-9-]*(\\.[a-z][a-z0-9-]*)+$",
        RegexOptions.Compiled);

    public RoleValidator()
    {
        RuleFor(role => role.DisplayName)
            .NotEmpty()
            .MaximumLength(DisplayNameMaximumLength);

        RuleFor(role => role.PermissionClaimKeys)
            .NotEmpty()
            .WithMessage("A role must grant at least one permission claim.");

        RuleFor(role => role.PermissionClaimKeys)
            // Ordinal: claim keys are identifiers, not user text, so a casing difference is a typo
            // to surface rather than a duplicate to fold away.
            .Must(keys => keys.Distinct(StringComparer.Ordinal).Count() == keys.Count)
            .WithMessage("A permission claim may be assigned to a role only once.")
            .When(role => role.PermissionClaimKeys is { Count: > 0 });

        RuleForEach(role => role.PermissionClaimKeys)
            .Matches(ClaimKeyPattern)
            // No braces in the text: FluentValidation's message formatter treats them as
            // placeholder syntax, so the AG-12 form is spelled out instead.
            .WithMessage("Permission claim keys must be lower-case resource.action identifiers.");
    }
}
