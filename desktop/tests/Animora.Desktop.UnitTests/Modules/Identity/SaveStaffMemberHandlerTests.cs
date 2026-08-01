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
/// SaveStaffMemberHandler's three seams substituted (DT-02/DT-03): the lookup-dependent rules
/// StaffValidator leaves to the handler — role existence, username uniqueness, and the SEC-17
/// namespacing guard — are each exercised without a DbContext or connection.
/// </summary>
public class SaveStaffMemberHandlerTests
{
    private static readonly Guid SystemRoleId = Guid.CreateVersion7();
    private static readonly Guid SubordinateRoleId = Guid.CreateVersion7();

    private readonly IStaffReadStore _staffReadStore = Substitute.For<IStaffReadStore>();
    private readonly IStaffWriteStore _staffWriteStore = Substitute.For<IStaffWriteStore>();
    private readonly IRoleReadStore _roleReadStore = Substitute.For<IRoleReadStore>();
    private readonly SaveStaffMemberHandler _handler;

    public SaveStaffMemberHandlerTests()
    {
        _handler = new SaveStaffMemberHandler(_staffReadStore, _staffWriteStore, _roleReadStore);

        _roleReadStore.GetByIdAsync(SystemRoleId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Role?>(new Role(SystemRoleId, "مدیر کلینیک", ["staff.manage"], MemberCount: 1, IsSystemRole: true)));

        _roleReadStore.GetByIdAsync(SubordinateRoleId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Role?>(new Role(SubordinateRoleId, "پذیرش", ["owners.read"], MemberCount: 0, IsSystemRole: false)));

        // Free by default; individual tests narrow this to simulate a taken username.
        _staffReadStore.FindIdByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Guid?>(null));
    }

    [Fact]
    public async Task A_validation_failure_returns_the_code_and_writes_nothing()
    {
        // An invalid Username format (contains a space) is StaffValidator's rejection, not the
        // handler's, so RoleId can be anything the handler never gets to look up.
        var command = new SaveStaffMemberCommand(null, "زهرا صادقی", "not valid", "09121234567", null, SubordinateRoleId, true);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IdentityErrors.ValidationFailed);
        await _roleReadStore.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _staffWriteStore.DidNotReceive().SaveAsync(Arg.Any<Guid>(), Arg.Any<IStaffInput>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_unknown_role_returns_role_not_found_and_writes_nothing()
    {
        var command = new SaveStaffMemberCommand(null, "زهرا صادقی", "petshop-zsadeghi", "09121234567", null, Guid.CreateVersion7(), true);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IdentityErrors.RoleNotFound);
        await _staffWriteStore.DidNotReceive().SaveAsync(Arg.Any<Guid>(), Arg.Any<IStaffInput>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_username_held_by_another_account_returns_username_already_taken_and_writes_nothing()
    {
        _staffReadStore.FindIdByUsernameAsync("petshop-zsadeghi", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Guid?>(Guid.CreateVersion7()));

        var command = new SaveStaffMemberCommand(null, "زهرا صادقی", "petshop-zsadeghi", "09121234567", null, SubordinateRoleId, true);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IdentityErrors.UsernameAlreadyTaken);
        await _staffWriteStore.DidNotReceive().SaveAsync(Arg.Any<Guid>(), Arg.Any<IStaffInput>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_create_assigns_a_fresh_uuid_v7_and_writes_through_the_store()
    {
        _staffReadStore.FindOwnerAdminUsernameAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>("petshop"));

        var command = new SaveStaffMemberCommand(null, "زهرا صادقی", "petshop-zsadeghi", "09121234567", null, SubordinateRoleId, true);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _staffWriteStore.Received(1).SaveAsync(result.Value, command, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_non_owner_admin_create_with_a_username_missing_the_sec17_prefix_is_rejected()
    {
        _staffReadStore.FindOwnerAdminUsernameAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>("petshop"));

        var command = new SaveStaffMemberCommand(null, "زهرا صادقی", "zsadeghi", "09121234567", null, SubordinateRoleId, true);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IdentityErrors.SubordinateUsernamePrefixRequired);
        await _staffWriteStore.DidNotReceive().SaveAsync(Arg.Any<Guid>(), Arg.Any<IStaffInput>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_system_role_create_is_exempt_from_the_sec17_prefix_check()
    {
        // No FindOwnerAdminUsernameAsync stub: the exemption branch must short-circuit before the
        // anchor lookup ever runs, which the DidNotReceive assertion below proves.
        var command = new SaveStaffMemberCommand(null, "امیر رحیمی", "petshop", "09121234567", null, SystemRoleId, true);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _staffReadStore.DidNotReceive().FindOwnerAdminUsernameAsync(Arg.Any<CancellationToken>());
        await _staffWriteStore.Received(1).SaveAsync(result.Value, command, true, Arg.Any<CancellationToken>());
    }
}
