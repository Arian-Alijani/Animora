namespace Animora.SharedKernel.Primitives;

/// <summary>
/// A failure identified by its stable <c>ERR-{MODULE}-{NNN}</c> code (CONV-13/14), optionally
/// carrying a developer-facing detail. Produced by handlers and carried by <see cref="Result"/>.
/// </summary>
/// <remarks>
/// There is no message catalog here on purpose: the code is the contract clients branch on
/// (INV-07), and its user-facing Persian text is resolved at the presentation edge from the stable
/// code (CONV-12). <see cref="Detail"/> is therefore never localized and never rendered as-is to a
/// user — it exists for logs and problem-details diagnostics.
/// <para>
/// The code itself is not validated against a pattern here: the authoritative list lives in
/// <c>Animora.Contracts/Errors</c> as constants, so a typo is caught by the compiler at the call
/// site rather than at runtime by a regex.
/// </para>
/// </remarks>
public sealed record Error
{
    /// <param name="code">A constant from <c>Animora.Contracts/Errors</c>, not an ad-hoc string.</param>
    /// <param name="detail">Optional non-localized diagnostic context.</param>
    public Error(string code, string? detail = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Code = code;
        Detail = detail;
    }

    /// <summary>The stable code clients switch on (CONV-15, INV-07).</summary>
    public string Code { get; }

    /// <summary>Non-localized diagnostic context for logs; never shown verbatim to a user.</summary>
    public string? Detail { get; }

    public override string ToString() => Detail is null ? Code : $"{Code}: {Detail}";
}
