using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Identity.Data;
using Animora.Desktop.Modules.Identity.Handlers;
using Animora.Desktop.Modules.Identity.Models;
using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Identity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Animora.Desktop.UnitTests.Modules.Identity;

/// <summary>
/// SaveRoleHandler's two seams substituted (DT-02/DT-03): the two lookup-dependent rules
/// RoleValidator leaves to the handler — catalog membership (SEC-09) and the system role's protected
/// claim (SEC-11) — are each exercised without a DbContext or connection.
/// </summary>
public class SaveRoleHandlerTests
{
    private static readonly Guid SystemRoleId = Guid.CreateVersion7();
    private static readonly Guid OrdinaryRoleId = Guid.CreateVersion7();

    private readonly IRoleReadStore _readStore = Substitute.For<IRoleReadStore>();
    private readonly IRoleWriteStore _writeStore = Substitute.For<IRoleWriteStore>();
    private readonly SaveRoleHandler _handler;

    public SaveRoleHandlerTests()
    {
        _handler = new SaveRoleHandler(_readStore, _writeStore);

        _readStore.GetByIdAsync(SystemRoleId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Role?>(new Role(SystemRoleId, "مدیر کلینیک", ["staff.manage"], MemberCount: 1, IsSystemRole: true)));

        _readStore.GetByIdAsync(OrdinaryRoleId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Role?>(new Role(OrdinaryRoleId, "پذیرش", ["owners.read"], MemberCount: 0, IsSystemRole: false)));
    }

    [Fact]
    public async Task A_validation_failure_returns_the_code_and_writes_nothing()
    {
        var command = new SaveRoleCommand(null, string.Empty, []);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IdentityErrors.ValidationFailed);
        await _writeStore.DidNotReceive().SaveAsync(Arg.Any<Guid>(), Arg.Any<IRoleInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_unknown_claim_key_is_rejected_and_writes_nothing()
    {
        // "front-desk.manage" matches RoleValidator's structural key format but is not in
        // PermissionCatalog — the catalog-membership check SEC-09 fixes as the handler's job.
        var command = new SaveRoleCommand(null, "پذیرش", ["owners.read", "front-desk.manage"]);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IdentityErrors.UnknownPermissionClaimKey);
        result.Error.Detail.Should().Be("front-desk.manage");
        await _writeStore.DidNotReceive().SaveAsync(Arg.Any<Guid>(), Arg.Any<IRoleInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Editing_the_system_role_without_its_protected_claim_is_rejected_and_writes_nothing()
    {
        var command = new SaveRoleCommand(SystemRoleId, "مدیر کلینیک", ["owners.read"]);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IdentityErrors.SystemRoleClaimProtected);
        await _writeStore.DidNotReceive().SaveAsync(Arg.Any<Guid>(), Arg.Any<IRoleInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Editing_the_system_role_while_keeping_its_protected_claim_succeeds()
    {
        var command = new SaveRoleCommand(SystemRoleId, "مدیر کلینیک", ["staff.manage", "owners.read"]);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(SystemRoleId);
        await _writeStore.Received(1).SaveAsync(SystemRoleId, command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_ordinary_role_edit_is_not_held_to_the_sec11_guard()
    {
        var command = new SaveRoleCommand(OrdinaryRoleId, "پذیرش", ["owners.read"]);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _writeStore.Received(1).SaveAsync(OrdinaryRoleId, command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_create_assigns_a_fresh_uuid_v7_and_writes_through_the_store()
    {
        var command = new SaveRoleCommand(null, "صندوق‌دار", ["cash-session.open", "cash-session.close"]);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _readStore.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _writeStore.Received(1).SaveAsync(result.Value, command, Arg.Any<CancellationToken>());
    }
}
