using System.Text.RegularExpressions;
using FluentValidation;

namespace Animora.SharedKernel.Validation.Clients;

/// <summary>
/// I/O-free structural and format rules for <see cref="IOwnerInput"/> (SH-05, CONV-18).
/// </summary>
/// <remarks>
/// 05-domain-model fixes the Owner aggregate boundary but not field-level formats (AG-02); the
/// mobile/landline/national-ID shapes enforced here are this phase's documented decision — see
/// <c>Roadmap/Desktop/phases/03-shared-kernel-primitives/TODO.md</c> item 9 — not an invented
/// default, and reviewable/adjustable without touching any other layer.
/// </remarks>
public sealed class OwnerValidator : AbstractValidator<IOwnerInput>
{
    // 11-digit Iranian mobile number, e.g. "09121234567". Operator prefixes vary and change over
    // time; the "09" lead plus fixed length is the only stable structural invariant.
    private static readonly Regex MobilePattern = new("^09\\d{9}$", RegexOptions.Compiled);

    // Iranian landline including area code, digits only, e.g. "02112345678" or "05112345678":
    // 10-11 digits starting with "0".
    private static readonly Regex LandlinePattern = new("^0\\d{9,10}$", RegexOptions.Compiled);

    public OwnerValidator()
    {
        RuleFor(owner => owner.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(owner => owner.MobileNumber)
            .NotEmpty()
            .Matches(MobilePattern)
            .WithMessage("Mobile number must be an 11-digit Iranian mobile number starting with 09.");

        RuleFor(owner => owner.LandlineNumber)
            .Matches(LandlinePattern)
            .WithMessage("Landline number must be 10-11 digits, including the area code.")
            .When(owner => !string.IsNullOrEmpty(owner.LandlineNumber));

        RuleFor(owner => owner.NationalId)
            .Must(IsValidNationalId)
            .WithMessage("National ID must be a valid 10-digit Iranian code-e-melli.")
            .When(owner => !string.IsNullOrEmpty(owner.NationalId));
    }

    // Standard Iranian national-ID checksum: mod-11 over the first 9 digits, weighted 10..2 from
    // the left, checked against the 10th digit. Also rejects the all-identical-digit codes
    // ("0000000000" etc.) that pass the arithmetic but were never actually issued.
    private static bool IsValidNationalId(string? nationalId)
    {
        if (nationalId is null || nationalId.Length != 10 || !nationalId.All(char.IsAsciiDigit))
        {
            return false;
        }

        if (nationalId.Distinct().Count() == 1)
        {
            return false;
        }

        var weightedSum = 0;
        for (var i = 0; i < 9; i++)
        {
            weightedSum += (nationalId[i] - '0') * (10 - i);
        }

        var remainder = weightedSum % 11;
        var checkDigit = nationalId[9] - '0';

        return remainder < 2 ? checkDigit == remainder : checkDigit == 11 - remainder;
    }
}
