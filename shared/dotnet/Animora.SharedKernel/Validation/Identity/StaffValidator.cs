using System.Text.RegularExpressions;
using FluentValidation;

namespace Animora.SharedKernel.Validation.Identity;

/// <summary>
/// I/O-free structural and format rules for <see cref="IStaffInput"/> (SH-05, CONV-18).
/// </summary>
/// <remarks>
/// Uniqueness of <see cref="IStaffInput.Username"/> and existence of <see cref="IStaffInput.RoleId"/>
/// are deliberately absent: both need a lookup, and a shared validator must behave identically
/// offline (SH-05). The handler checks them against its own store and answers with an
/// <c>ERR-IDENTITY-{NNN}</c> code instead.
/// <para>
/// 10-security-and-access-control fixes the RBAC model but no field-level formats (AG-02); the name,
/// username, mobile and e-mail shapes enforced here are this phase's documented decision — see
/// <c>Roadmap/Desktop/phases/04-identity-auth-screens/TODO.md</c> — not an invented default, and
/// reviewable without touching any other layer.
/// </para>
/// </remarks>
public sealed class StaffValidator : AbstractValidator<IStaffInput>
{
    /// <summary>Shortest accepted <see cref="IStaffInput.Username"/>.</summary>
    public const int UsernameMinimumLength = 3;

    /// <summary>Longest accepted <see cref="IStaffInput.Username"/>.</summary>
    public const int UsernameMaximumLength = 64;

    // Lower-case ASCII, starting with a letter, with dot/underscore/hyphen allowed between
    // characters. The leading-letter rule keeps a username from being read as a number or an id,
    // and excluding Persian letters is what makes case-folding and storage collation a non-issue.
    private static readonly Regex UsernamePattern = new(
        $"^[a-z][a-z0-9._-]{{{UsernameMinimumLength - 1},{UsernameMaximumLength - 1}}}$",
        RegexOptions.Compiled);

    public StaffValidator()
    {
        RuleFor(staff => staff.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(staff => staff.Username)
            .NotEmpty()
            .Matches(UsernamePattern)
            .WithMessage(
                $"Username must be {UsernameMinimumLength}-{UsernameMaximumLength} lower-case ASCII characters starting with a letter.");

        RuleFor(staff => staff.MobileNumber)
            .NotEmpty()
            .Matches(IranianContactFormats.Mobile)
            .WithMessage(IranianContactFormats.MobileMessage);

        RuleFor(staff => staff.Email)
            // FluentValidation's default e-mail check is a "contains an @ with something either
            // side" test, which is the only assertion that can be made without sending a message —
            // a stricter pattern would reject deliverable addresses.
            .EmailAddress()
            // RFC 5321's maximum path length; anything longer cannot be delivered anyway.
            .MaximumLength(254)
            .When(staff => !string.IsNullOrEmpty(staff.Email));

        RuleFor(staff => staff.RoleId)
            .NotEmpty()
            .WithMessage("A staff member must be assigned exactly one primary role.");
    }
}
