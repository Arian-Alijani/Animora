using System.Text.RegularExpressions;

namespace Animora.SharedKernel.Validation;

/// <summary>
/// The Iranian phone-number shapes more than one validator enforces, held in one place so the two
/// rule sets can never drift apart (INV-02).
/// </summary>
internal static class IranianContactFormats
{
    // 11-digit Iranian mobile number, e.g. "09121234567". Operator prefixes vary and change over
    // time; the "09" lead plus fixed length is the only stable structural invariant.
    // [0-9] rather than \d on purpose: .NET's \d also matches Persian-Indic digits, so "09۱۲۱۲۳۴۵۶۷"
    // would pass and reach storage un-normalized. Persian digits are converted at the UI edge
    // (CONV-05); what arrives here must already be ASCII.
    internal static readonly Regex Mobile = new("^09[0-9]{9}$", RegexOptions.Compiled);

    // Iranian landline including area code, digits only, e.g. "02112345678" or "05112345678":
    // 10-11 digits starting with "0".
    internal static readonly Regex Landline = new("^0[0-9]{9,10}$", RegexOptions.Compiled);

    internal const string MobileMessage =
        "Mobile number must be an 11-digit Iranian mobile number starting with 09.";

    internal const string LandlineMessage =
        "Landline number must be 10-11 digits, including the area code.";
}
