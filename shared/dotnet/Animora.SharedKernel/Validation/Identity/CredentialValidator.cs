using FluentValidation;

namespace Animora.SharedKernel.Validation.Identity;

/// <summary>
/// I/O-free structural rules for <see cref="ICredentialInput"/> — enough to tell an incomplete form
/// from a submitted one, and nothing more (SH-05, CONV-18).
/// </summary>
/// <remarks>
/// There is deliberately no minimum length, complexity rule or character class here. Password policy
/// is the server's, applied where a password is *set* (SEC-03); repeating it at the sign-in edge
/// would lock out any account whose password predates a policy change and would leak the policy to
/// an untrusted client (SEC-01). The upper bounds below are anti-abuse limits on the input buffer,
/// not policy.
/// </remarks>
public sealed class CredentialValidator : AbstractValidator<ICredentialInput>
{
    /// <summary>Upper bound on the submitted username, mirroring <see cref="StaffValidator.UsernameMaximumLength"/>.</summary>
    public const int UsernameMaximumLength = StaffValidator.UsernameMaximumLength;

    /// <summary>Upper bound on the submitted password; an input longer than this cannot be a real one.</summary>
    public const int PasswordMaximumLength = 128;

    public CredentialValidator()
    {
        RuleFor(credential => credential.Username)
            .NotEmpty()
            .MaximumLength(UsernameMaximumLength);

        RuleFor(credential => credential.Password)
            .NotEmpty()
            .MaximumLength(PasswordMaximumLength);
    }
}
