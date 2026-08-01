using Animora.SharedKernel.Primitives;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.UnitTests.SharedKernel;

public class TenantIdTests
{
    [Fact]
    public void New_generates_a_uuid_v7_value()
    {
        TenantId tenantId = TenantId.New();

        // CONV-01/02: client-generated UUIDv7. The version nibble is the first character of a
        // canonical "D" form's third group, e.g. 019... -7xxx- ....
        tenantId.ToGuid().ToString("D")[14].Should().Be('7');
    }

    [Fact]
    public void New_never_returns_the_empty_value()
    {
        TenantId.New().IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void New_returns_a_distinct_value_per_call()
    {
        TenantId.New().Should().NotBe(TenantId.New());
    }

    [Fact]
    public void Empty_equals_the_default_value_and_is_flagged_empty()
    {
        // A struct's default is unavoidable (a field or array slot starts there), so "unset" and
        // default(TenantId) must be the same state — otherwise there would be two invalid values.
        TenantId.Empty.Should().Be(default(TenantId));
        TenantId.Empty.IsEmpty.Should().BeTrue();
        TenantId.Empty.ToGuid().Should().Be(Guid.Empty);
    }

    [Fact]
    public void FromGuid_round_trips_through_ToGuid()
    {
        var key = Guid.CreateVersion7();

        TenantId.FromGuid(key).ToGuid().Should().Be(key);
    }

    [Fact]
    public void Explicit_conversions_match_FromGuid_and_ToGuid()
    {
        var key = Guid.CreateVersion7();

        var tenantId = (TenantId)key;

        tenantId.Should().Be(TenantId.FromGuid(key));
        ((Guid)tenantId).Should().Be(key);
    }

    [Fact]
    public void Equality_is_by_underlying_key()
    {
        var key = Guid.CreateVersion7();

        TenantId.FromGuid(key).Should().Be(TenantId.FromGuid(key));
        TenantId.FromGuid(key).GetHashCode().Should().Be(TenantId.FromGuid(key).GetHashCode());
        TenantId.FromGuid(key).Should().NotBe(TenantId.New());
    }

    [Fact]
    public void ToString_renders_the_bare_key_without_the_wrapper_shape()
    {
        var key = Guid.CreateVersion7();

        // CONV-21: a log line must carry a value that can be grepped against the database column,
        // not the record struct's "TenantId { Value = ... }" default rendering.
        TenantId.FromGuid(key).ToString().Should().Be(key.ToString("D"));
    }
}
