using System.Globalization;
using Animora.SharedKernel;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.UnitTests.SharedKernel;

public class MoneyTests
{
    [Fact]
    public void Zero_equals_the_default_value()
    {
        Money.Zero.Should().Be(default(Money));
        Money.Zero.Amount.Should().Be(0m);
    }

    [Fact]
    public void Addition_and_subtraction_agree_with_their_named_alternates()
    {
        var left = new Money(120_000m);
        var right = new Money(45_500m);

        (left + right).Should().Be(left.Add(right)).And.Be(new Money(165_500m));
        (left - right).Should().Be(left.Subtract(right)).And.Be(new Money(74_500m));
        (-left).Should().Be(left.Negate()).And.Be(new Money(-120_000m));
    }

    [Fact]
    public void Multiplication_by_a_quantity_is_commutative()
    {
        var unitPrice = new Money(250_000m);

        (unitPrice * 3m).Should().Be(new Money(750_000m));
        (3m * unitPrice).Should().Be(unitPrice.Multiply(3m));
    }

    [Fact]
    public void Comparison_operators_order_by_amount()
    {
        var smaller = new Money(10m);
        var larger = new Money(20m);

        (smaller < larger).Should().BeTrue();
        (smaller <= larger).Should().BeTrue();
        (larger > smaller).Should().BeTrue();
        (larger >= smaller).Should().BeTrue();
        (smaller <= new Money(10m)).Should().BeTrue();
        (smaller >= new Money(10m)).Should().BeTrue();
    }

    [Fact]
    public void CompareTo_supports_sorting_a_ledger_column()
    {
        Money[] amounts = [new(30m), new(-10m), new(20m)];

        Array.Sort(amounts);

        amounts.Should().Equal(new Money(-10m), new Money(20m), new Money(30m));
    }

    [Fact]
    public void Non_generic_CompareTo_sorts_null_first_and_rejects_a_foreign_type()
    {
        IComparable amount = new Money(1m);

        amount.CompareTo(null).Should().Be(1);

        var act = () => amount.CompareTo("1");

        act.Should().Throw<ArgumentException>();
    }

    // Decimal literals cannot be attribute constants, so the cases are typed here rather than via
    // [InlineData], which would round-trip each amount through a double first.
    public static TheoryData<decimal, decimal> PersistenceRoundingCases =>
        new()
        {
            // Midpoints (CONV-08, FIN-20): half-to-even, not half-away-from-zero.
            { 2.345m, 2.34m },
            { 2.355m, 2.36m },
            { -2.345m, -2.34m },
            { -2.355m, -2.36m },
            // Non-midpoints round normally.
            { 2.346m, 2.35m },
            { 2.344m, 2.34m },
            // Already at or below the storage scale: unchanged.
            { 1_500.00m, 1_500.00m },
            { 0m, 0m },
        };

    [Theory]
    [MemberData(nameof(PersistenceRoundingCases))]
    public void RoundForPersistence_applies_bankers_rounding_at_two_decimal_places(decimal amount, decimal expected)
    {
        new Money(amount).RoundForPersistence().Should().Be(new Money(expected));
    }

    [Fact]
    public void Arithmetic_keeps_full_precision_until_persistence_is_requested()
    {
        // CONV-07's decimal(18,2) is a storage scale, not a calculation scale: a 9% VAT line on
        // 1,234.56 is 111.1104, and truncating that before the invoice total is summed is exactly
        // the per-step bias RoundForPersistence exists to avoid.
        Money tax = new Money(1_234.56m) * 0.09m;

        tax.Amount.Should().Be(111.1104m);
        tax.RoundForPersistence().Amount.Should().Be(111.11m);
    }

    [Fact]
    public void Rounding_once_at_the_end_differs_from_rounding_every_step()
    {
        var component = new Money(0.005m);

        Money roundedOnce = (component + component + component).RoundForPersistence();
        Money roundedEachStep =
            component.RoundForPersistence() + component.RoundForPersistence() + component.RoundForPersistence();

        // 0.015 -> 0.02 (half-to-even on an odd digit), whereas each 0.005 alone -> 0.00. The two
        // results differ by a whole cent on three lines; CONV-08 mandates the first form, and this
        // test fails the moment an operator starts rounding on its own.
        roundedOnce.Amount.Should().Be(0.02m);
        roundedEachStep.Amount.Should().Be(0m);
    }

    [Fact]
    public void RoundForPersistence_is_idempotent()
    {
        Money once = new Money(2.345m).RoundForPersistence();

        once.RoundForPersistence().Should().Be(once);
    }

    [Fact]
    public void ToString_is_culture_invariant_so_logs_do_not_depend_on_the_machine()
    {
        CultureInfo original = CultureInfo.CurrentCulture;

        try
        {
            // fa-IR formats decimals with Persian digits; diagnostics must not (CONV-05 keeps
            // Persian formatting at the UI edge).
            CultureInfo.CurrentCulture = new CultureInfo("fa-IR");

            new Money(1_234.5m).ToString().Should().Be("1234.5");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
