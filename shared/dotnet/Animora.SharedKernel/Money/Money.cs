using System.Globalization;

namespace Animora.SharedKernel;

/// <summary>
/// An amount of money in Rials (IRR), the single currency the product handles (CONV-09), held as
/// <see langword="decimal"/> so no binary floating-point type can ever touch money (INV-05).
/// </summary>
/// <remarks>
/// The type is namespaced at the shared-kernel root rather than under a <c>Money</c> namespace so
/// that <c>Money.Zero</c> can never be read as a namespace reference at a call site.
/// <para>
/// There is no currency member and no conversion to or from <see langword="decimal"/>: arithmetic
/// stays inside the type, and the only way out is <see cref="Amount"/> at a persistence or
/// formatting boundary (Rial/Toman display is the UI's job, CONV-05/FIN-19).
/// </para>
/// <para>
/// Instances carry the full precision of every intermediate calculation; the fixed
/// <c>decimal(18,2)</c> scale (CONV-07) is applied once, by
/// <see cref="RoundForPersistence"/>, at the moment the value is written (CONV-08). Rounding on
/// each operation instead would accumulate a per-step bias that no ledger reconciliation could
/// explain.
/// </para>
/// </remarks>
public readonly record struct Money : IComparable<Money>, IComparable
{
    // decimal(18,2) / numeric(18,2) — CONV-07.
    private const int PersistenceScale = 2;

    /// <param name="amount">The amount in Rials.</param>
    public Money(decimal amount) => Amount = amount;

    /// <summary>Nothing owed and nothing paid; also <c>default(Money)</c>.</summary>
    public static Money Zero => default;

    /// <summary>The amount in Rials, at full calculation precision until it is persisted.</summary>
    public decimal Amount { get; }

    /// <summary>
    /// The value as it will be stored: scaled to two decimal places with banker's rounding
    /// (CONV-08, FIN-20). Call this once, at the persistence boundary — never mid-calculation.
    /// </summary>
    public Money RoundForPersistence() => new(Math.Round(Amount, PersistenceScale, MidpointRounding.ToEven));

    /// <summary>Named alternate for <c>operator +</c>.</summary>
    public Money Add(Money other) => new(Amount + other.Amount);

    /// <summary>Named alternate for the binary <c>operator -</c>.</summary>
    public Money Subtract(Money other) => new(Amount - other.Amount);

    /// <summary>Named alternate for the unary <c>operator -</c>.</summary>
    public Money Negate() => new(-Amount);

    /// <summary>Named alternate for <c>operator *</c>; the factor is a quantity or rate, never money.</summary>
    public Money Multiply(decimal factor) => new(Amount * factor);

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator -(Money left, Money right) => left.Subtract(right);

    public static Money operator -(Money value) => value.Negate();

    public static Money operator *(Money left, decimal right) => left.Multiply(right);

    public static Money operator *(decimal left, Money right) => right.Multiply(left);

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    public int CompareTo(Money other) => Amount.CompareTo(other.Amount);

    int IComparable.CompareTo(object? obj) => obj switch
    {
        null => 1,
        Money other => CompareTo(other),
        _ => throw new ArgumentException($"Object must be of type {nameof(Money)}.", nameof(obj)),
    };

    /// <summary>Culture-invariant digits for logs and diagnostics; user-facing text is formatted at the UI edge (CONV-05).</summary>
    public override string ToString() => Amount.ToString(CultureInfo.InvariantCulture);
}
